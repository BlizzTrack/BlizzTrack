using Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BNetLib.Ribbit;
using BNetLib.Ribbit.Commands;

namespace BlizzTrack.Pages
{
    [IgnoreAntiforgeryToken]
    public class TermModel : PageModel
    {
        private readonly BNetClient _bnetClient;
        private readonly ISummary _summary;

        public TermModel(BNetClient bnetClient, ISummary summary)
        {
            _bnetClient = bnetClient;
            _summary = summary;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync(string command)
        {
            var cmd = command.Split(" ");

            return cmd.First().ToLower() switch
            {
                "help" or "?" => Content(await CreateHelpfunction()),
                "get" or "g" => Content(await CallBlizzard(cmd)),
                _ => Content("Unknown Command")
            };
        }

        private async Task<string> CallBlizzard(string[] command)
        {
            if (command.Length != 2) return "Usage: get {command}";

            return await _bnetClient.Call(command.Last());
        }

        private async Task<string> CreateHelpfunction()
        {
            var latest = await _summary.Latest();

            var sb = new StringBuilder();

            sb.AppendLine("get v1/summary");

            foreach (var item in latest.Content)
            {
                AbstractCommand l = item.Flags switch
                {
                    "versions" or "version" => new VersionCommand(item.Product),
                    "cdn" or "cdns" => new CDNCommand(item.Product),
                    "bgdl" => new BGDLCommand(item.Product),
                    _ => null
                };

                if (l != null)
                    sb.AppendLine($"get {l}");
            }

            return sb.ToString();
        }
    }
}