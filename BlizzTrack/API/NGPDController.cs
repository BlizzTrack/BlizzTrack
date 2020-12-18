﻿using BNetLib.Networking.Commands;
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
    public class NGPDController : ControllerBase
    {
        private readonly ISummary _summary;
        private readonly IVersions _versions;
        private readonly ICDNs _cdns;
        private readonly IBGDL _bgdl;
        private readonly DBContext _dbContext;
        private readonly ILogger<NGPDController> _logger;
        private readonly IGameConfig _gameConfig;

        public NGPDController(ISummary summary, IVersions versions, ICDNs cdns, IBGDL bgdl, DBContext dbContext, ILogger<NGPDController> logger, IGameConfig gameConfig)
        {
            _summary = summary;
            _versions = versions;
            _cdns = cdns;
            _bgdl = bgdl;
            _dbContext = dbContext;
            _logger = logger;
            _gameConfig = gameConfig;
        }

        [HttpGet("summary/changes")]
        public async Task<IActionResult> GetChanges()
        {
            var scheme = Request.Scheme;
            if (Request.Host.Host.Contains("blizztrack", StringComparison.OrdinalIgnoreCase))
            {
                scheme = "https";
            }

            var Manifests = await _summary.Take(2);

            var latest = Manifests.First();
            var previous = Manifests.Last();

            var SummaryDiff = new List<object>();
            var configs = await _gameConfig.In(latest.Content.Select(x => x.Product).ToArray());

            foreach (var item in latest.Content)
            {
                var x = previous.Content.FirstOrDefault(x => x.Product == item.Product && x.Flags == item.Flags);
                if (x == null || x.Seqn != item.Seqn)
                {
                    SummaryDiff.Add(new {
                        name = x.GetName(),
                        x.Product,
                        x.Flags,
                        x.Seqn,
                        encrypted = configs.FirstOrDefault(f => f.Code == x.Product)?.Config.Encrypted,
                        logos = configs.FirstOrDefault(f => f.Code == x.Product)?.Logos,
                        relations = new
                        {
                            view = Url.Action("Get", "ngpd", new { code = x.Product, file = x.Flags, seqn = x.Seqn }, scheme),
                            seqn = Url.Action("Get", "ngpd", new { code = x.Product, file = "seqn", filter = x.Flags }, scheme),
                        }
                    });
                }
            }

            return Ok(new
            {
                changes = SummaryDiff,
                latest,
                previous,
            });
        }

        [HttpGet("{code?}/{file?}")]
        public async Task<IActionResult> Get(string code = "summary", string file = "supported", [FromQuery] string filter = default, [FromQuery] int? seqn = null)
        {
            var scheme = Request.Scheme;
            if (Request.Host.Host.Contains("blizztrack", StringComparison.OrdinalIgnoreCase))
            {
                scheme = "https";
            }

            AbstractCommand cmd = new SummaryCommand();
            if (!code.Equals("summary", StringComparison.OrdinalIgnoreCase))
            {
                code = code.ToLower();
                object result = null;

                switch (file)
                {
                    case "version" or "versions":
                        {
                            cmd = new VersionCommand(code);
                            var data = await _versions.Single(code, seqn);
                            if (data == null || data.Content == null) return NotFound(new { error = "Not found" });
                            var config = await _gameConfig.Get(code);

                            var ver = data.Content.Select(x => new
                            {
                                region_name = x.GetName(),
                                x.Buildconfig,
                                x.Buildid,
                                x.Cdnconfig,
                                x.Keyring,
                                x.Region,
                                x.Versionsname,
                                x.Productconfig,
                            }).ToList();

                            result = new
                            {
                                data.Name,
                                data.Indexed,
                                data.Seqn,
                                code,
                                encrypted = config.Config.Encrypted,
                                logos = config.Logos,
                                command = cmd.ToString(),
                                data = ver
                            };
                            break;
                        }
                    case "cdn" or "cdns":
                        {
                            cmd = new CDNCommand(code);
                            var data = await _cdns.Single(code, seqn);
                            if (data == null || data.Content == null) return NotFound(new { error = "Not found" });
                            var config = await _gameConfig.Get(code);

                            var ver = data.Content.Select(x => new
                            {
                                region_name = x.GetName(),
                                region = x.Name,
                                x.Path,
                                hosts = x.Hosts.Split(" "),
                                servers = x.Servers.Split(" "),
                                x.ConfigPath
                            }).ToList();

                            result = new
                            {
                                data.Name,
                                data.Indexed,
                                data.Seqn,
                                code,
                                encrypted = config.Config.Encrypted,
                                logos = config.Logos,
                                command = cmd.ToString(),
                                data = ver
                            };
                            break;
                        }
                    case "bgdl":
                        {
                            cmd = new BGDLCommand(code);
                            var data = await _bgdl.Single(code, seqn);
                            if (data == null || data.Content == null) return NotFound(new { error = "Not found" });
                            var config = await _gameConfig.Get(code);

                            var ver = data.Content.Select(x => new
                            {
                                region_name = x.GetName(),
                                x.Buildconfig,
                                x.Buildid,
                                x.Cdnconfig,
                                x.Keyring,
                                x.Region,
                                x.Versionsname,
                                x.Productconfig,
                            }).ToList();

                            result = new
                            {
                                data.Name,
                                data.Indexed,
                                data.Seqn,
                                code,
                                encrypted = config.Config.Encrypted,
                                logos = config.Logos,
                                command = cmd.ToString(),
                                data = ver
                            };
                            break;
                        }
                    case "seqn":
                        {
                            cmd = null;
                            object data = null;
                            switch (filter?.ToLower())
                            {
                                case "version" or "versions":
                                    {
                                        var ver = await _versions.Seqn(code);
                                        if (ver == null) return NotFound(new { error = "Not found" });
                                        var config = await _gameConfig.Get(code);

                                        var f = ver.Select(data => new
                                        {
                                            data.Seqn,
                                            data.Indexed,
                                            view = Url.Action("Get", "ngpd", new { code, file = "versions", seqn = data.Seqn }, scheme),
                                        }).ToList();

                                        data = new
                                        {
                                            name = BNetLib.Helpers.GameName.Get(code),
                                            code = code.ToLower(),
                                            encrypted = config.Config.Encrypted,
                                            logos = config.Logos,
                                            latest = f.First(),
                                            data = f
                                        };
                                        break;
                                    }
                                case "cdn" or "cdns":
                                    {
                                        var cdn = await _cdns.Seqn(code);
                                        if (cdn == null)
                                            return NotFound(new { error = "Not found" });
                                        var config = await _gameConfig.Get(code);

                                        var f = cdn.Select(data => new
                                        {
                                            data.Seqn,
                                            data.Indexed,
                                            view = Url.Action("Get", "ngpd", new { code, file = "cdn", seqn = data.Seqn }, scheme),
                                        }).ToList();

                                        data = new
                                        {
                                            name = BNetLib.Helpers.GameName.Get(code),
                                            code = code.ToLower(),
                                            encrypted = config.Config.Encrypted,
                                            logos = config.Logos,
                                            latest = f.First(),
                                            data = f
                                        };
                                        break;
                                    }
                                case "bgdl":
                                    {
                                        var bgdl = await _bgdl.Seqn(code);
                                        if (bgdl == null)
                                            return NotFound(new { error = "Not found" });
                                        var config = await _gameConfig.Get(code);

                                        var f = bgdl.Select(data => new
                                        {
                                            data.Seqn,
                                            data.Indexed,
                                            view = Url.Action("Get", "ngpd", new { code, file = "bdgl", seqn = data.Seqn }, scheme),
                                        }).ToList();

                                        data = new
                                        {
                                            name = BNetLib.Helpers.GameName.Get(code),
                                            code = code.ToLower(),
                                            encrypted = config.Config.Encrypted,
                                            logos = config.Logos,
                                            latest = f.First(),
                                            data = f
                                        };
                                        break;
                                    }
                                default:
                                    return BadRequest(new
                                    {
                                        error = "Unknown filter",
                                        accepted = new
                                        {
                                            versions = Url.Action("Get", "ngpd", new { code, file = "seqn", filter = "versions" }, scheme),
                                            cdns = Url.Action("Get", "ngpd", new { code, file = "seqn", filter = "cdns" }, scheme),
                                            bgdl = Url.Action("Get", "ngpd", new { code, file = "seqn", filter = "bgdl" }, scheme),
                                        }
                                    });
                            }

                            result = data;
                            break;
                        }
                    default:
                        result = new
                        {
                            versions = Url.Action("Get", "ngpd", new { code, file = "version" }, scheme),
                            cdns = Url.Action("Get", "ngpd", new { code, file = "cdns" }, scheme),
                            bgdl = Url.Action("Get", "ngpd", new { code, file = "bgdl" }, scheme),
                            seqn = new
                            {
                                versions = Url.Action("Get", "ngpd", new { code, file = "seqn", filter = "versions" }, scheme),
                                cdns = Url.Action("Get", "ngpd", new { code, file = "seqn", filter = "cdns" }, scheme),
                                bgdl = Url.Action("Get", "ngpd", new { code, file = "seqn", filter = "bgdl" }, scheme),
                            },
                            supported = Url.Action("Get", "ngpd", new { code, file = "supported" }, scheme),
                        };
                        break;
                }

                return Ok(result);
            }
            else
            {
                object result = null;

                switch (file?.ToLower())
                {
                    case "seqn":
                        {
                            var summary = await _summary.Seqn();
                            if (summary == null) return NotFound(new { error = "Not found" });

                            var f = summary.Select(data => new
                            {
                                data.Seqn,
                                data.Indexed,
                                view = Url.Action("Get", "ngpd", new { code, file = "view", seqn = data.Seqn }, scheme),
                            }).ToList();

                            result = new
                            {
                                name = "Summary",
                                code = code.ToLower(),
                                latest = f.First(),
                                data = f
                            };
                            break;
                        }
                    case "view":
                    default:
                        {
                            var data = await _summary.Single(seqn);
                            if (data == null) return NotFound(new { error = "Not Found" });
                            var configs = await _gameConfig.In(data.Content.Select(x => x.Product).ToArray());

                            result = new
                            {
                                data.Seqn,
                                command = cmd.ToString(),
                                name = "summary",
                                code = code.ToLower(),
                                data = data.Content.Select(x => new
                                {
                                    name = x.GetName(),
                                    x.Product,
                                    x.Flags,
                                    x.Seqn,
                                    encrypted = configs.FirstOrDefault(f => f.Code == x.Product)?.Config.Encrypted,
                                    logos = configs.FirstOrDefault(f => f.Code == x.Product)?.Logos,
                                    relations = new
                                    {
                                        view = Url.Action("Get", "ngpd", new { code = x.Product, file = x.Flags, seqn = x.Seqn }, scheme),
                                        seqn = Url.Action("Get", "ngpd", new { code = x.Product, file = "seqn", filter = x.Flags }, scheme),
                                    }
                                }).ToList().Where(x => filter == default || x.Product.Contains(filter, StringComparison.OrdinalIgnoreCase))
                            };
                            break;
                        }
                }


                return Ok(result);
            }
        }
    }
}