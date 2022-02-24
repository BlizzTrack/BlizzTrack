using Core.Models;
using Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BGDL = BNetLib.Ribbit.Models.BGDL;
using GameChildren = Core.Models.GameChildren;
using Summary = BNetLib.Ribbit.Models.Summary;
using Versions = BNetLib.Ribbit.Models.Versions;

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

        private readonly ISummary _summary;
        private readonly IGameConfig _gameConfig;
        private readonly IVersions _versions;
        private readonly IBGDL _bgdl;
        private readonly IGameCompanies _gameCompanies;
        private readonly DBContext _dbContext;

        public IndexModel(ISummary summary, IGameConfig gameConfig, IVersions versions, DBContext dbContext, IBGDL bgdl, IGameCompanies gameCompanies)
        {
            _summary = summary;
            _gameConfig = gameConfig;
            _versions = versions;
            _dbContext = dbContext;
            _bgdl = bgdl;
            _gameCompanies = gameCompanies;
        }

        public List<Manifest<Summary[]>> Manifests;
        
        public readonly List<(Summary newest, Summary previous)> SummaryDiff = new();

        [BindProperty(SupportsGet = true, Name = "search")]
        public string Search { get;  set; }

        public List<Core.Models.GameConfig> Configs;

        public List<GameCompany> Companies;

        public List<Manifest<Versions[]>> Versions;

        public List<Manifest<BGDL[]>> BgdLs;

        public List<GameChildren> GameChildrenList;

        public List<UpdateTimes> GameVersions { get; } = new();
        
        public async Task OnGetAsync()
        {
            Companies = await _gameCompanies.All();

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

            GameChildrenList = await _dbContext.GameChildren.ToListAsync();
            
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