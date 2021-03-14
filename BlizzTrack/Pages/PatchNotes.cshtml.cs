using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlizzTrack.Helpers;
using Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.FeatureManagement.Mvc;

namespace BlizzTrack.Pages
{
    [FeatureGate(nameof(FeatureFlags.PatchNotes))]
    public class PatchNotesModel : PageModel
    {
        private readonly IGameParents _gameParents;

        private readonly Services.IPatchnotes _patchNotes;

        private readonly Services.IBlizzardAlerts _blizzardAlerts;

        public PatchNotesModel(IGameParents gameParents, Services.IPatchnotes patchNotes, Services.IBlizzardAlerts blizzardAlerts)
        {
            _gameParents = gameParents;
            _patchNotes = patchNotes;
            _blizzardAlerts = blizzardAlerts;
        }

        [BindProperty(SupportsGet = true, Name = "slug")]
        public string Slug { get; set; }

        [BindProperty(SupportsGet = true, Name = "game_type")]
        public string GameType { get; set; }

        [BindProperty(SupportsGet = true, Name = "language")]
        public string Langauge { get; set; } = "en-us";

        public Core.Models.GameParents GameParent;

        public Models.PatchNoteData PatchNotes;

        public string Alert = string.Empty;

        public List<Models.PatchNoteBuild> BuildList { get; set; }

        public async Task<IActionResult> OnGetAsync([FromQuery(Name = "build_time")] long? buildTime = null)
        {
            GameParent = await _gameParents.Get(Slug);
            if(GameParent == null || !GameParent.PatchNoteAreas.Contains(GameType.ToLower())) return NotFound();

            PatchNotes = await _patchNotes.Get(GameParent.Code, GameType, buildTime != null ? new DateTime(buildTime.Value) : null, Langauge);
            if (PatchNotes == null) return NotFound();

            BuildList = await _patchNotes.GetBuildDates(GameParent.Code, GameType, Langauge);

            if(GameParent.Slug == "overwatch" && GameType.Equals("experimental"))
            {
                Alert = await _blizzardAlerts.ExperimentalOnline();
            }

            return Page();
        }
    }
}
