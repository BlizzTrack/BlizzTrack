﻿using BNetLib.Networking.Commands;
using Core.Models;
using Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public NGPDController(ISummary summary, IVersions versions, ICDNs cdns, IBGDL bgdl, DBContext dbContext)
        {
            _summary = summary;
            _versions = versions;
            _cdns = cdns;
            _bgdl = bgdl;
            _dbContext = dbContext;
        }

        [HttpGet("{code?}/{file?}")]
        public async Task<IActionResult> Get(string code = "summary", string file = "supported", [FromQuery] string filter = default, [FromQuery] int? seqn = null)
        {
            AbstractCommand cmd = new SummaryCommand();
            if (!code.Equals("summary", System.StringComparison.OrdinalIgnoreCase))
            {
                code = code.ToLower();
                object result = null;

                switch (file)
                {
                    case "version" or "versions":
                        {
                            cmd = new VersionCommand(code);
                            var data = await _versions.Single(code, seqn);
                            if (data == null || data.Content == null)
                                return NotFound(new { error = "Not found" });

                            var ver = data?.Content.Select(x => new
                            {
                                region_name =   x.GetName(),
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
                                name = data.Name,
                                data.Indexed,
                                data.Seqn,
                                code,
                                command = cmd.ToString(),
                                data = ver
                            };
                            break;
                        }
                    case "cdn" or "cdns":
                        {
                            cmd = new CDNCommand(code);
                            var data = await _cdns.Single(code, seqn);
                            if (data == null || data.Content == null)
                                return NotFound(new { error = "Not found" });

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
                                name = data.Name,
                                data.Indexed,
                                data.Seqn,
                                code,
                                command = cmd.ToString(),
                                data = ver
                            };
                            break;
                        }
                    case "bgdl":
                        {
                            cmd = new BGDLCommand(code);
                            var data = await _bgdl.Single(code, seqn);
                            if (data == null || data.Content == null)
                                return NotFound(new { error = "Not found" });

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
                                name = data.Name,
                                data.Indexed,
                                data.Seqn,
                                code,
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
                                        if (ver == null)
                                            return NotFound(new { error = "Not found" });

                                        var f = ver.Select(data => new
                                        {
                                            data.Seqn,
                                            data.Indexed,
                                            view = Url.Action("Get", "ngpd", new { code, file = "versions", seqn = data.Seqn }, Request.Scheme),
                                        }).ToList();

                                        data = new
                                        {
                                            name = BNetLib.Helpers.GameName.Get(code),
                                            code = code.ToLower(),
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

                                        var f = cdn.Select(data => new
                                        {
                                            data.Seqn,
                                            data.Indexed,
                                            view = Url.Action("Get", "ngpd", new { code, file = "cdn", seqn = data.Seqn }, Request.Scheme),
                                        }).ToList();

                                        data = new
                                        {
                                            name = BNetLib.Helpers.GameName.Get(code),
                                            code = code.ToLower(),
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

                                        var f = bgdl.Select(data => new
                                        {
                                            data.Seqn,
                                            data.Indexed,
                                            view = Url.Action("Get", "ngpd", new { code, file = "bdgl", seqn = data.Seqn }, Request.Scheme),
                                        }).ToList();

                                        data = new
                                        {
                                            name = BNetLib.Helpers.GameName.Get(code),
                                            code = code.ToLower(),
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
                                            versions = Url.Action("Get", "ngpd", new { code, file = "seqn", filter = "versions" }, Request.Scheme),
                                            cdns = Url.Action("Get", "ngpd", new { code, file = "seqn", filter = "cdns" }, Request.Scheme),
                                            bgdl = Url.Action("Get", "ngpd", new { code, file = "seqn", filter = "bgdl" }, Request.Scheme),
                                        }
                                    });
                            }

                            result = data;
                            break;
                        }
                    default:
                        result = new
                        {
                            versions = Url.Action("Get", "ngpd", new { code, file = "version" }, Request.Scheme),
                            cdns = Url.Action("Get", "ngpd", new { code, file = "cdns" }, Request.Scheme),
                            bgdl = Url.Action("Get", "ngpd", new { code, file = "bgdl" }, Request.Scheme),
                            seqn = new
                            {
                                versions = Url.Action("Get", "ngpd", new { code, file = "seqn", filter = "versions" }, Request.Scheme),
                                cdns = Url.Action("Get", "ngpd", new { code, file = "seqn", filter = "cdns" }, Request.Scheme),
                                bgdl = Url.Action("Get", "ngpd", new { code, file = "seqn", filter = "bgdl" }, Request.Scheme),
                            },
                            supported = Url.Action("Get", "ngpd", new { code, file = "supported" }, Request.Scheme),
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
                            if (summary == null)
                                return NotFound(new { error = "Not found" });

                            var f = summary.Select(data => new
                            {
                                data.Seqn,
                                data.Indexed,
                                view = Url.Action("Get", "ngpd", new { code, file = "seqn", seqn = data.Seqn }, Request.Scheme),
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
                    default:
                        {
                            var data = await _summary.Single(seqn);
                            if (data == null) return NotFound(new { error = "Not Found" });

                            // TODO: Move this to it's own system at some point
                            var items = new List<(DateTime? indexed, string code, string file)>();
                            foreach(var item in data.Content)
                            {
                                items.Add((null, item.Product, item.Flags));
                            }

                            var versionArray = items.Where(x => x.file == "versions").Select(x => x.code).ToArray();
                            var versionsUpdated = await _dbContext.Versions.GroupBy(x => x.Code, (x, y) => new
                            {
                                Indexed = y.Max(x => x.Indexed),
                                Code = x
                            }).ToListAsync();

                            var bgdlArray = items.Where(x => x.file == "bgdl").Select(x => x.code).ToArray();
                            var bgdlUpdated = await _dbContext.BGDL.GroupBy(x => x.Code, (x, y) => new
                            {
                                Indexed = y.Max(x => x.Indexed),
                                Code = x
                            }).ToListAsync();


                            var cdnArray = items.Where(x => x.file == "cdns" || x.file == "cdn").Select(x => x.code).ToArray();
                            var cdnUpdated = await _dbContext.CDN.GroupBy(x => x.Code, (x, y) => new
                            {
                                Indexed = y.Max(x => x.Indexed),
                                Code = x
                            }).ToListAsync();


                            for (var i = 0; i < items.Count; i++)
                            {
                                var item = items[i];
                                item.indexed = item.file switch
                                {
                                    "versions" or "version" => versionsUpdated.FirstOrDefault(x => x.Code == item.code)?.Indexed,
                                    "cdn" or "cdns" => cdnUpdated.FirstOrDefault(x => x.Code == item.code)?.Indexed,
                                    "bdgl" => bgdlUpdated.FirstOrDefault(x => x.Code == item.code)?.Indexed,
                                    _ => null
                                };

                                items[i] = item;
                            }

                            result = new
                            {
                                data.Seqn,
                                command = cmd.ToString(),
                                name = "Summary",
                                code = code.ToLower(),
                                data = data.Content.Select(x => new
                                {
                                    name = x.GetName(),
                                    x.Product,
                                    x.Flags,
                                    x.Seqn,
                                    indexed = items.FirstOrDefault(s => s.code == x.Product && s.file == x.Flags).indexed,
                                    relations = new
                                    {
                                        view = Url.Action("Get", "ngpd", new { code = x.Product, file = x.Flags, seqn = x.Seqn }, Request.Scheme),
                                        seqn = Url.Action("Get", "ngpd", new { code = x.Product, file = "seqn", filter = x.Flags }, Request.Scheme),
                                    }
                                }).ToList().Where(x => filter == default ? true : x.Product.Contains(filter, System.StringComparison.OrdinalIgnoreCase))
                            };
                            break;
                        }
                }


                return Ok(result);
            }
        }
    }
}