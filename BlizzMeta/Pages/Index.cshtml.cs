using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BNetLib.Ribbit.Models;
using Core.Extensions;
using Core.Models;
using Core.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using BGDL = BNetLib.Ribbit.Models.BGDL;
using Summary = BNetLib.Ribbit.Models.Summary;
using Versions = BNetLib.Ribbit.Models.Versions;

namespace BlizzMeta.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IGameChildren _gameChildren;
        private readonly IVersions _versions;
        private readonly IBGDL _bgdl;
        private readonly ICDNs _cdNs;
        private readonly ISummary _summary;

        public IndexModel(ILogger<IndexModel> logger,  IGameChildren gameChildren, IVersions versions, IBGDL bgdl, ICDNs cdNs, ISummary summary)
        {
            _logger = logger;
            _gameChildren = gameChildren;
            _versions = versions;
            _bgdl = bgdl;
            _cdNs = cdNs;
            _summary = summary;
        }

        public List<Core.Models.GameChildren> Children;
        
        public List<Manifest<Versions[]>> Versions;

        public List<Manifest<BGDL[]>> Bgdls;
        
        public List<Manifest<CDN[]>> Cdns;

        public Manifest<Summary[]> Manifests;

        public async Task OnGetAsync()
        {
            Children = await _gameChildren.All();
            Children = Children.OrderByAlphaNumeric(x => x.Code).Where(x => x.GameConfig != null).ToList();
            
            Manifests = await _summary.Latest();
            var latest = Manifests.Content;
            
            Versions = await _versions.MultiBySeqn(latest.Where(x => x.Flags == "versions").Select(x => x.Seqn).ToList());
            Bgdls = await _bgdl.MultiBySeqn(latest.Where(x => x.Flags == "bgdl").Select(x => x.Seqn).ToList());
            Cdns = await _cdNs.MultiBySeqn(latest.Where(x => x.Flags == "cdns" || x.Flags == "cdn").Select(x => x.Seqn).ToList());
        }
    }
}
