using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BlizzTrack.Pages.Docs
{
    public class BotModel : PageModel
    {
        public readonly string BotDoc;

        private readonly IWebHostEnvironment _hostingEnvironment;

        public BotModel(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            BotDoc = System.IO.File.ReadAllText(System.IO.Path.Combine(_hostingEnvironment.WebRootPath, "docs", "discord.md"));
        }

        public void OnGet()
        {
        }
    }
}