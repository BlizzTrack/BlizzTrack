using BlizzTrack.Helpers;
using Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.FeatureManagement.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlizzTrack.Pages.Admin
{
    [FeatureGate(nameof(FeatureFlags.AdminPanel))]
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IGameConfig _gameConfig;

        [BindProperty(SupportsGet = true, Name = "search")]
        public string Search { get; set; }

        public IndexModel(IGameConfig gameConfig)
        {
            _gameConfig = gameConfig;
        }

        public List<Core.Models.GameConfig> Configs;

        public async Task OnGetAsync()
        {
            Configs = await _gameConfig.All();
        }
    }
}
