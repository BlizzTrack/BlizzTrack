using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Core.Extensions;
using Core.Models;
using Core.Services;
using Microsoft.EntityFrameworkCore;
using Tooling.Attributes;
using GameChildren = Core.Models.GameChildren;

namespace Tooling.Tools
{
    [Tool(Name =  "Game Childern Importer", Order = 1, Disabled = false)]
    public class GameChildrenLoader : ITool
    {
        private readonly IGameParents _gameParents;
        private readonly ISummary _summary;
        private readonly DBContext _dbContext;
        private readonly IGameConfig _gameConfig;

        public GameChildrenLoader(IGameParents gameParents, ISummary summary, DBContext dbContext, IGameConfig gameConfig)
        {
            _gameParents = gameParents;
            _summary = summary;
            _dbContext = dbContext;
            _gameConfig = gameConfig;
        }
        
        public async Task Start()
        {
            var latestGames = await _summary.Latest();
            var parents = await _gameParents.All();
            var gameConfigs = await _gameConfig.All();
            
            foreach (var item in latestGames.Content.Select(x => x.Product).Distinct())
            {
                var parent = parents.FirstOrDefault(x => item.StartsWith(x.Code) || x.ChildrenOverride.Contains(item));
                var config = gameConfigs.FirstOrDefault(x => x.Code == item);  
                
                if (parent != null)
                {
                    await _dbContext.Entry(parent).Collection(x => x.Children).LoadAsync();

                    Debugger.Break();

                    var name = string.IsNullOrEmpty(config?.Name) ? BNetLib.Helpers.GameName.Get(item) : config?.Name;
                    var f = new GameChildren
                    {
                        Code = item,
                        ParentCode = parent.Code,
                        Parent = parent,
                        GameConfig = config,
                        Name = name,
                        Slug = name.Slugify()
                    };
                    if (parent.Children.FirstOrDefault(x => x.Code == f.Code) == null)
                    {
                        await _dbContext.GameChildren.AddAsync(f);
                    }
                    else
                    {
                        var child = await _dbContext.GameChildren.FirstOrDefaultAsync(x => x.Code == f.Code);
                        child.Parent = parent;
                        child.GameConfig = config;
                        child.Name = name;
                        child.Slug = name.Slugify();
                    }

                    // parent.Children.Add(f);
                }
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}