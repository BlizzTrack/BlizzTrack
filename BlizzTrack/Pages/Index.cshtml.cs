using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Models;
using Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BlizzTrack.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ISummary _summary;

        public IndexModel(ISummary summary)
        {
            _summary = summary;
        }

        public List<Manifest<BNetLib.Models.Summary[]>> Manifests = null;
        public List<(BNetLib.Models.Summary newest, BNetLib.Models.Summary previous)> SummaryDiff = new List<(BNetLib.Models.Summary newest, BNetLib.Models.Summary previous)>();

        [BindProperty(SupportsGet = true, Name = "search")]
        public string Search { get; set; }

        public async Task OnGetAsync()
        {
            Manifests = await _summary.Take(2);

            var latest = Manifests.First().Content;
            var previous = Manifests.Last().Content;

            foreach(var item in latest)
            {
                var exist = previous.FirstOrDefault(x => x.Product == item.Product && x.Flags == item.Flags);
                if (exist == null) continue; // TODO
                if(exist.Seqn != item.Seqn)
                {
                    SummaryDiff.Add((item, exist));
                }
            }
        }
    }
}
