using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Models;
using Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BlizzTrack.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ISummary _summary;
        private readonly IGameConfig _gameConfig;
        private readonly IVersions _versions;
        private readonly IGameParents _gameParents;

        public IndexModel(ISummary summary, IGameConfig gameConfig, IVersions versions, IGameParents gameParents)
        {
            _summary = summary;
            _gameConfig = gameConfig;
            _versions = versions;
            _gameParents = gameParents;
        }

        public List<Manifest<BNetLib.Models.Summary[]>> Manifests = null;
        public List<(BNetLib.Models.Summary newest, BNetLib.Models.Summary previous)> SummaryDiff = new List<(BNetLib.Models.Summary newest, BNetLib.Models.Summary previous)>();

        [BindProperty(SupportsGet = true, Name = "search")]
        public string Search { get; set; }

        public List<Core.Models.GameConfig> Configs;

        public List<Core.Models.GameParents> Parents;

        public async Task OnGetAsync()
        {
            Parents = await _gameParents.All();

            Manifests = await _summary.Take(2);

            var latest = Manifests.First().Content;
            var previous = Manifests.Last().Content;

            Configs = await _gameConfig.In(latest.Where(x => x.Flags == "versions").Select(x => x.Product).ToArray());
                
            foreach (var item in latest)
            {
                var exist = previous.FirstOrDefault(x => x.Product == item.Product && x.Flags == item.Flags);
                if (exist == null)
                {
                    SummaryDiff.Add((item, null));
                    continue;
                }// TODO
                if(exist.Seqn != item.Seqn)
                {
                    SummaryDiff.Add((item, exist));
                }
            }
        }
    }
}
