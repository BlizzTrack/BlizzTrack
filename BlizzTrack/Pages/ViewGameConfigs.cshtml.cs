using Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace BlizzTrack.Pages
{
    public class ViewGameManifestRawModel : PageModel
    {
        private readonly ICatalog _catalog;

        public ViewGameManifestRawModel(ICatalog catalog)
        {
            _catalog = catalog;
        }

        [BindProperty(SupportsGet = true, Name = "hash")]
        public string Hash { get; set; }

        public Core.Models.Catalog Catalog { get; set; }

        public async Task OnGetAsync()
        {
            Catalog = await _catalog.Get(Hash);
        }
    }
}
