using Core.Models;
using Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlizzTrack.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class UIController : ControllerBase
    {
        public record NavItem(string Name, string Code, List<NavItemInfo> Items);
        public record NavItemInfo(string Name, string Product, List<SeqnItem> Flags);
        public record SeqnItem(string File, int seqn);

        private readonly ISummary _summary;
        private readonly ILogger<UIController> _logger;

        public UIController(ILogger<UIController> logger, ISummary summary)
        {
            _logger = logger;
            _summary = summary;
        }

        [HttpGet("home")]
        public async Task<IActionResult> GameList([FromQuery]int? seqn = null)
        {
            Manifest<BNetLib.Models.Summary[]> sum;
            if (seqn == null) sum = await _summary.Latest();
            else sum = await _summary.Single(seqn);

            if(sum == null || sum.Content.Length == 0) return NotFound(new { error = "Not Found" });

            return Ok(Create(sum.Content.ToList()));
        }

        public List<NavItem> Create(List<BNetLib.Models.Summary> items)
        {
            var p = BNetLib.Helpers.GameName.Prefix.Keys.OrderBy(x => x).ToList();

            var res = new List<NavItem>();

            var itemsAdded = new List<BNetLib.Models.Summary>();

            foreach (var prefix in p)
            {
                if (prefix == "storm") continue;

                var i = new List<NavItemInfo>();

                switch (prefix)
                {
                    case "agent":
                    case "bts":
                    case "catalogs":
                        continue;
                    case "bna":
                        /*
                        var its = items.Where(x => x.Product == "bna" || x.Product == "catalogs" || x.Product == "agent" || x.Product == "bts");
                        var collection = its as BNetLib.Models.Summary[] ?? its.ToArray();
                        i.AddRange(collection.Select(x => new NavItemInfo(
                            x.GetName(),
                            x.Product,
                            x.Flags,
                            x.Seqn
                        )));
                        itemsAdded.AddRange(collection);

                        res.Add(new NavItem(BNetLib.Helpers.GameName.Get(prefix), prefix, i));
                        */
                        continue;
                }

                foreach (var item in items.Where(item => item.Product.ToLower().Replace("_", "")
                    .StartsWith(prefix, StringComparison.CurrentCultureIgnoreCase)))
                {
                    if (item.Product.StartsWith("wow_classic") && prefix == "wow")
                    {
                        itemsAdded.Add(item);
                        continue;
                    }

                    itemsAdded.Add(item);

                    var exist = i.FirstOrDefault(x => x.Product == item.Product);
                    if (exist == null) {
                        exist = new NavItemInfo(
                            item.GetName(),
                            item.Product,
                            new List<SeqnItem>() { new SeqnItem(item.Flags, item.Seqn) }
                        );
                        i.Add(exist);
                    }
                    else {
                        exist.Flags.Add(new SeqnItem(item.Flags, item.Seqn));
                        exist.Flags.OrderByDescending(x => x.File);
                    }
                }

                if (i.Count > 0)
                    res.Add(new NavItem(BNetLib.Helpers.GameName.Get(prefix), prefix, i));
            }

            foreach (var item in itemsAdded)
            {
                items.Remove(item);
            }

            if (items.Count <= 0)
                return res;

            {
                var i = new List<NavItemInfo>();
                foreach (var item in items)
                {

                    var exist = i.FirstOrDefault(x => x.Product == item.Product);
                    if (exist == null)
                        i.Add(new NavItemInfo(
                            item.GetName(),
                            item.Product,
                            new List<SeqnItem>() { new SeqnItem(item.Flags, item.Seqn) }
                        ));
                    else
                    {
                        exist.Flags.Add(new SeqnItem(item.Flags, item.Seqn));
                        exist.Flags.OrderByDescending(x => x.File);
                    }
                }

                res.Add(new NavItem(BNetLib.Helpers.GameName.Get("unknown"), "unknown", i));
            }

            return res;
        }
    }
}
