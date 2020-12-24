using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlizzTrack.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.FeatureManagement.Mvc;

namespace BlizzTrack.Pages.Admin.Parents
{
    [FeatureGate(nameof(FeatureFlags.AdminPanel))]
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
