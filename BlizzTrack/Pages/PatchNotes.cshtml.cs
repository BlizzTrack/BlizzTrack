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

        public PatchNotesModel(IGameParents gameParents, Services.IPatchnotes patchNotes)
        {
            _gameParents = gameParents;
            _patchNotes = patchNotes;
        }

        [BindProperty(SupportsGet = true, Name = "slug")]
        public string Slug { get; set; }

        [BindProperty(SupportsGet = true, Name = "game_type")]
        public string GameType { get; set; }

        public Core.Models.GameParents GameParent;

        public Models.PatchNoteData PatchNotes;

        public async Task<IActionResult> OnGetAsync()
        {
            GameParent = await _gameParents.GetBySlug(Slug);
            if(GameParent == null || !GameParent.PatchNoteAreas.Contains(GameType.ToLower())) return NotFound();

            PatchNotes = await _patchNotes.Get(GameParent.Code, GameType);
            if (PatchNotes == null) return NotFound();

            return Page();
        }
    }
}
