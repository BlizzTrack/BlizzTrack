using Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Services
{
    public interface IGameParents
    {
        Task Add(Models.GameParents config);

        Task<List<Models.GameParents>> All();

        Task<Models.GameParents> Get(string code);

        Task Update(Models.GameParents config);
    }

    public class GameParents : IGameParents
    {
        private readonly DBContext _dbContext;

        public GameParents(DBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Add(Models.GameParents config)
        {
            _dbContext.GameParents.Add(config);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<Models.GameParents>> All()
        {
            return await _dbContext.GameParents
                .Include(x => x.Children)
                .ThenInclude(x => x.GameConfig)
                .ToListAsync();
        }

        public async Task<Models.GameParents> Get(string code)
        {
            code = code.ToLower();
            return await _dbContext.GameParents
                .Include(x => x.Children)
                .ThenInclude(x => x.GameConfig)
                .FirstOrDefaultAsync(x => code == x.Code || x.Slug == code.ToLower() || code.StartsWith(x.Code) || x.ChildrenOverride.Contains(code.ToLower()));
        }

        public async Task Update(Models.GameParents config)
        {
            _dbContext.GameParents.Update(config);
            await _dbContext.SaveChangesAsync();
        }
    }
}