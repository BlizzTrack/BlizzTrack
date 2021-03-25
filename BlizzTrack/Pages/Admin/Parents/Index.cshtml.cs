using BlizzTrack.Helpers;
using Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.FeatureManagement.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Summary = BNetLib.Models.Summary;

namespace BlizzTrack.Pages.Admin.Parents
{
    [FeatureGate(nameof(FeatureFlags.AdminPanel))]
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IGameParents _gameParents;
        private readonly ISummary _summary;

        public IndexModel(IGameParents gameParents, ISummary summary)
        {
            _gameParents = gameParents;
            _summary = summary;
        }

        public List<Core.Models.GameParents> Parents;
        public List<Summary> Children;

        public async Task OnGetAsync()
        {
            Parents = await _gameParents.All();
            Parents = Parents.OrderBy(x => x.Code).ToList();

            var f = await _summary.Latest();
            Children = f.Content.ToList();
        }
    }
}