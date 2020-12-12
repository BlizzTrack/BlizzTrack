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
        public class NavItemInfo
        {
            public NavItemInfo(string Name, string Product, List<SeqnItem> Flags)
            {
                this.Name = Name;
                this.Product = Product;
                this.Flags = Flags;
            }

            public string Name { get; set; }
            public string Product { get; set; }
            public List<SeqnItem> Flags { get; set; }
        }

        public record SeqnItem(string File, int seqn);

        private readonly ISummary _summary;
        private readonly ILogger<UIController> _logger;

        public UIController(ILogger<UIController> logger, ISummary summary)
        {
            _logger = logger;
            _summary = summary;
        }

        #region /Home
        [HttpGet("home")]
        public async Task<IActionResult> GameList([FromQuery]int? seqn = null)
        {
            Manifest<BNetLib.Models.Summary[]> sum;
            if (seqn == null) sum = await _summary.Latest();
            else sum = await _summary.Single(seqn);

            if(sum == null || sum.Content.Length == 0) return NotFound(new { error = "Not Found" });

            return Ok(Create(sum.Content.ToList()));
        }

        private List<NavItem> Create(List<BNetLib.Models.Summary> items)
        {
            var p = BNetLib.Helpers.GameName.Prefix.Keys.OrderBy(x => x).ToList();

            var res = new List<NavItem>();

            var itemsAdded = new List<BNetLib.Models.Summary>();

            foreach (var prefix in p)
            {

                var i = new List<NavItemInfo>();

                switch (prefix)
                {
                    case "agent":
                    case "bts":
                    case "catalogs":
                        continue;
                    case "bna":
                        var its = items.Where(x => x.Product == "bna" || x.Product == "catalogs" || x.Product == "agent" || x.Product == "bts");

                        foreach (var item in its)
                        {
                            var exist = i.FirstOrDefault(x => x.Product == item.Product);
                            if (exist == null)
                            {
                                exist = new NavItemInfo(
                                    item.GetName(),
                                    item.Product,
                                    new List<SeqnItem>() { new SeqnItem(item.Flags, item.Seqn) }
                                );
                                i.Add(exist);
                            }
                            else
                            {
                                exist.Flags.Add(new SeqnItem(item.Flags, item.Seqn));
                                exist.Flags = exist.Flags.OrderByDescending(x => x.File).ToList();
                            }

                            itemsAdded.Add(item);
                        }

                        res.Add(new NavItem(BNetLib.Helpers.GameName.Get(prefix), prefix, i));
                       
                        continue;
                }

                foreach (var item in items.Where(item => item.Product.ToLower().Replace("_", "")
                    .StartsWith(prefix, StringComparison.CurrentCultureIgnoreCase)))
                {
                    if (item.Product == "storm")
                    {
                        itemsAdded.Add(item);
                        continue;
                    }

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
                        exist.Flags = exist.Flags.OrderByDescending(x => x.File).ToList();
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
                        exist.Flags = exist.Flags.OrderByDescending(x => x.File).ToList();
                    }
                }

                res.Add(new NavItem(BNetLib.Helpers.GameName.Get("unknown"), "unknown", i));
            }

            return res;
        }
        #endregion

        #region /game/:code
        [HttpGet("game/{code}")]
        public async Task<IActionResult> Manifests([FromServices]IVersions versionService, [FromServices] IBGDL bgdlService, [FromServices] ICDNs cdnService, string code)
        {
            var versions = await versionService.Latest(code);
            var bgdl = await bgdlService.Latest(code);
            var cdn = await cdnService.Latest(code);
            var latest = await _summary.Latest();

            return Ok(new
            {
                metadata = new {
                    name = BNetLib.Helpers.GameName.Get(code),
                    code = code.ToLower(),
                    files = latest.Content.Where(x => x.Product.Equals(code, StringComparison.OrdinalIgnoreCase)).Select(x => new
                    {
                        x.Flags,
                        x.Seqn
                    }).OrderByDescending(x => x.Flags).ToList(),
                },
                files = new
                {
                    versions = versions == null ? null : new
                    {
                        versions.Indexed,
                        versions.Seqn,
                        command = new BNetLib.Networking.Commands.VersionCommand(code).ToString(),
                        Content = versions.Content.Select(x => new
                        {
                            region = x.GetName(),
                            x.Versionsname,
                            x.Buildid,
                            x.Buildconfig,
                            x.Productconfig,
                            x.Cdnconfig
                        }).ToList(),
                    },
                    bgdl = bgdl == null ? null : new
                    {
                        bgdl.Indexed,
                        bgdl.Seqn,
                        command = new BNetLib.Networking.Commands.BGDLCommand(code).ToString(),
                        Content = bgdl.Content.Select(x => new
                        {
                            region = x.GetName(),
                            x.Versionsname,
                            x.Buildid,
                            x.Buildconfig,
                            x.Productconfig,
                            x.Cdnconfig
                        }).ToList(),
                    },
                    cdn = cdn == null ? null : new
                    {
                        cdn.Indexed,
                        cdn.Seqn,
                        command = new BNetLib.Networking.Commands.CDNCommand(code).ToString(),
                        content = cdn.Content.Select(x => new
                        {
                            name = x.GetName(),
                            x.Path,
                            hosts = x.Hosts.Split(" "),
                            servers = x.Servers.Split(" "),
                            x.ConfigPath
                        }).ToList()
                    },
                }
            });
        }

        #endregion
    }
}
