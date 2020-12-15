using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
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
