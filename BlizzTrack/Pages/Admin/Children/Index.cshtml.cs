using BlizzTrack.Helpers;
using Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.FeatureManagement.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Summary = BNetLib.Models.Summary;

namespace BlizzTrack.Pages.Admin.Children
{
    [FeatureGate(nameof(FeatureFlags.AdminPanel))]
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IGameChildren _gameChildren;

        public IndexModel(IGameChildren gameChildren)
        {
            _gameChildren = gameChildren;
        }

        public List<Core.Models.GameChildren> Children;
        
        public async Task OnGetAsync()
        {
            Children = await _gameChildren.All();
            Children = Children.OrderBy(x => x.Code).ToList();
        }
    }
}