using BlizzTrack.Helpers;
using Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.FeatureManagement.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlizzTrack.Pages.Admin.Parents
{
    [FeatureGate(nameof(FeatureFlags.AdminPanel))]
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IGameParents _gameParents;

        public IndexModel(IGameParents gameParents)
        {
            _gameParents = gameParents;
        }

        public List<Core.Models.GameParents> Parents;

        public async Task OnGetAsync()
        {
            Parents = await _gameParents.All();
            Parents = Parents.OrderBy(x => x.Code).ToList();
        }
    }
}