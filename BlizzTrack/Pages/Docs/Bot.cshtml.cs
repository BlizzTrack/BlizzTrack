using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace BlizzTrack.Pages.Docs
{
    public class BotModel : PageModel
    {
        private readonly IWebHostEnvironment _hostingEnvironment;

        public BotDocs Docs { get; set; }

        public BotModel(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;

            Docs = Newtonsoft.Json.JsonConvert.DeserializeObject<BotDocs>(System.IO.File.ReadAllText(System.IO.Path.Combine(_hostingEnvironment.WebRootPath, "docs", "commands.json")));
            // System.IO.File.ReadAllText(System.IO.Path.Combine(_hostingEnvironment.WebRootPath, "docs", "discord.md")
        }

        public void OnGet()
        {
#if DEBUG
            Docs = Newtonsoft.Json.JsonConvert.DeserializeObject<BotDocs>(System.IO.File.ReadAllText(System.IO.Path.Combine(_hostingEnvironment.WebRootPath, "docs", "commands.json")));
#endif
        }
    }

    public record BotDocs(string prefix, List<BotCommand> commands);

    public record BotCommand(string command, string description, string usage, string[] aliases);
}