using Core.Models;
using Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlizzTrack.API;

namespace BlizzTrack.Pages
{
    public class IndexModel : PageModel
    {
        public class UpdateTimes
        {
            public string Type { get; set; }
            public string Code { get; set; }
            public DateTime Updated { get; set; }
        }

        public class CatalogEntryType
        {
            public string Code { get; set; }

            public bool Activison { get; set; }
        }

        private readonly ISummary _summary;
        private readonly IGameConfig _gameConfig;
        private readonly IVersions _versions;
        private readonly IBGDL _bgdl;
        private readonly IGameParents _gameParents;
        private readonly DBContext _dbContext;

        public IndexModel(ISummary summary, IGameConfig gameConfig, IVersions versions, IGameParents gameParents, DBContext dbContext, IBGDL bgdl)
        {
            _summary = summary;
            _gameConfig = gameConfig;
            _versions = versions;
            _gameParents = gameParents;
            _dbContext = dbContext;
            _bgdl = bgdl;
        }

        public List<Manifest<BNetLib.Models.Summary[]>> Manifests;
        
        public readonly List<(BNetLib.Models.Summary newest, BNetLib.Models.Summary previous)> SummaryDiff = new();

        [BindProperty(SupportsGet = true, Name = "search")]
        public string Search { get; set; }

        public List<Core.Models.GameConfig> Configs;

        public List<Core.Models.GameParents> Parents;

        public List<Manifest<BNetLib.Models.Versions[]>> Versions;

        public List<Manifest<BNetLib.Models.BGDL[]>> BgdLs;

        public List<UpdateTimes> GameVersions { get; } = new();

        public List<CatalogEntryType> CatalogEntries { get; set; }

        public async Task OnGetAsync()
        {
            Parents = await _gameParents.All();
            Parents = Parents.Where(x => x.Visible == true).ToList();

            CatalogEntries  = await _dbContext.Catalogs
                .OrderByDescending(x => x.Indexed)
                .Where(x => Parents.Select(gameParents => gameParents.ManifestID).Contains(x.Name))
                .Select(x => new CatalogEntryType
                {
                    Code = x.Name,
                    Activison = x.Activision
                }).Distinct().ToListAsync();

            Manifests = await _summary.Take(2);

            var latest = Manifests.First().Content;
            var previous = Manifests.Last().Content;

            Configs = await _gameConfig.In(latest.Where(x => x.Flags == "versions").Select(x => x.Product).ToArray());

            var verSeqn = latest.Where(x => x.Flags == "versions").Select(x => x.Seqn).ToList();
            var vers = await _dbContext.Versions.OrderByDescending(x => x.Seqn).Where(s => verSeqn.Contains(s.Seqn)).Select(x => new UpdateTimes
            {
                Type = "versions",
                Code = x.Code,
                Updated = x.Indexed
            }).Distinct().ToListAsync();

            var bgdlCodes = latest.Where(x => x.Flags == "bgdl").Select(x => x.Seqn).ToList();
            var bgdl = await _dbContext.BGDL.OrderByDescending(x => x.Seqn).Where(s => bgdlCodes.Contains(s.Seqn)).Select(x => new UpdateTimes
            {
                Type = "bgdl",
                Code = x.Code,
                Updated = x.Indexed
            }).Distinct().ToListAsync();

            GameVersions.AddRange(vers);
            GameVersions.AddRange(bgdl);

            Versions = await _versions.MultiBySeqn(latest.Where(x => x.Flags == "versions").Select(x => x.Seqn).ToList());
            BgdLs = await _bgdl.MultiBySeqn(latest.Where(x => x.Flags == "bgdl").Select(x => x.Seqn).ToList());
            
            foreach (var item in latest)
            {
                var exist = previous.FirstOrDefault(x => x.Product == item.Product && x.Flags == item.Flags);
                if (exist == null)
                {
                    SummaryDiff.Add((item, null));
                    continue;
                }
                if (exist.Seqn != item.Seqn)
                {
                    SummaryDiff.Add((item, exist));
                }
            }
        }
    }
}