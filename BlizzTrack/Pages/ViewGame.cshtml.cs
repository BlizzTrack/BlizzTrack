using BlizzTrack.Services;
using Core.Models;
using Core.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlizzTrack.Pages
{
    public class ViewGameModel : PageModel
    {
        private readonly IVersions _versions;
        private readonly ICDNs _cdns;
        private readonly IBGDL _bgdl;
        private readonly ISummary _summary;
        private readonly IGameConfig _gameConfig;
        private readonly IBlizzardAlerts _blizzardAlerts;
        private readonly IGameParents _gameParents;

        public ViewGameModel(ISummary summary, IBGDL bgdl, ICDNs cdns, IVersions versions, IGameConfig gameConfig, IBlizzardAlerts blizzardAlerts, IGameParents gameParents)
        {
            _summary = summary;
            _bgdl = bgdl;
            _cdns = cdns;
            _versions = versions;
            _gameConfig = gameConfig;
            _blizzardAlerts = blizzardAlerts;
            _gameParents = gameParents;
        }

        [BindProperty(SupportsGet = true, Name = "code")]
        public string Code { get; set; }

        [BindProperty(SupportsGet = true, Name = "file")]
        public new string File { get; set; }

        [BindProperty(SupportsGet = true, Name = "latest-seqn")]
        public int? LatestSeqn { get; set; } = null;

        [BindProperty(SupportsGet = true, Name = "previous-seqn")]
        public int? PreviousSeqn { get; set; } = null;

        public BNetLib.Models.Summary Meta { get; set; }

        public object Manifest { get; set; }

        public Core.Models.GameConfig GameConfig { get; set; }

        public Core.Models.GameParents GameParent { get; set; }

        public string Alert { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            var summary = await _summary.Latest();
            var exist = summary.Content.FirstOrDefault(x => x.Product.Equals(Code, System.StringComparison.OrdinalIgnoreCase) && x.Flags.Equals(File, System.StringComparison.OrdinalIgnoreCase));
            if (exist == null) return NotFound();

            GameParent = await _gameParents.Get(Code);

            GameConfig = await _gameConfig.Get(exist.Product);

            if(GameConfig == null)
            {
                GameConfig = new Core.Models.GameConfig()
                    {
                        Config = new ConfigItems()
                        {
                            Encrypted = true,
                        },
                        Code = Code.ToLower()
                    };

            }
            if (!string.IsNullOrEmpty(GameConfig.ServiceURL))
                Alert = await _blizzardAlerts.Get(GameConfig.ServiceURL);

            Meta = exist;

            Manifest = exist.Flags switch
            {
                "versions" or "version" => await GetVersions(exist.Product),
                "bgdl" => await GetBGDL(exist.Product),
                _ => null
            };

            return Page();
       }

        private async Task<List<Manifest<BNetLib.Models.Versions[]>>> GetVersions(string product)
        {
            var data = await _versions.Take(product, 2);
            if (LatestSeqn != null)
            {
                var f = await _versions.Single(Code, LatestSeqn);
                if (f != null) data[0] = f;
            }

            if (PreviousSeqn != null)
            {
                var f = await _versions.Single(Code, PreviousSeqn);
                if (f != null) {
                    if (data.Count == 1)
                        data.Add(f);
                    else
                        data[1] = f;
                }
            }

            return data;
        }

        private async Task<List<Manifest<BNetLib.Models.BGDL[]>>> GetBGDL(string product)
        {
            var data = await _bgdl.Take(product, 2);
            if (LatestSeqn != null)
            {
                var f = await _bgdl.Single(Code, LatestSeqn);
                if (f != null) data[0] = f;
            }

            if (PreviousSeqn != null)
            {
                var f = await _bgdl.Single(Code, PreviousSeqn);
                if (f != null)
                {
                    if (data.Count == 1)
                        data.Add(f);
                    else
                        data[1] = f;
                }
            }

            return data;
        }
    }
}
