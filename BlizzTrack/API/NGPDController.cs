﻿using Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using BNetLib.Ribbit.Commands;

namespace BlizzTrack.API
{
    [Obsolete, DataContract, JsonConverter(typeof(StringEnumConverter))]
    public enum FileType
    {
        [EnumMember(Value = "help")]
        Help,

        [EnumMember(Value = "versions")]
        Versions,

        [EnumMember(Value = "bgdl")]
        BGDL,

        [EnumMember(Value = "cdn")]
        CDN,

        [EnumMember(Value = "seqn")]
        Seqn,
    }

    [Obsolete, DataContract, JsonConverter(typeof(StringEnumConverter))]
    public enum FileFilter
    {
        [EnumMember(Value = "versions")]
        Versions,

        [EnumMember(Value = "bgdl")]
        BGDL,

        [EnumMember(Value = "cdn")]
        CDN
    }

    [Obsolete, JsonConverter(typeof(StringEnumConverter))]
    public enum SummaryFilter
    {
        [EnumMember(Value = "seqn")]
        Seqn,

        [EnumMember(Value = "all")]
        All
    }

    [Route("api/ngpd"), ApiController, ApiExplorerSettings(GroupName = "Game Versions (Deprecated)"),
     Produces("application/json"), Obsolete]
    public class NGPDController : ControllerBase
    {
        private readonly IVersions _versions;
        private readonly ICDNs _cdns;
        private readonly IBGDL _bgdl;
        private readonly ILogger<NGPDController> _logger;
        private readonly IGameConfig _gameConfig;
        private readonly IGameParents _gameParents;

        public NGPDController(IVersions versions, ICDNs cdns, IBGDL bgdl, ILogger<NGPDController> logger, IGameConfig gameConfig, IGameParents gameParents)
        {
            _versions = versions;
            _cdns = cdns;
            _bgdl = bgdl;
            _logger = logger;
            _gameConfig = gameConfig;
            _gameParents = gameParents;
        }

        /// <summary>
        ///     Returns latest changes for the given filters
        /// </summary>
        /// <returns>Returns latest changes for the given filters</returns>
        /// <response code="200">Returns latest items for given seqn</response>
        /// <param name="code">The game slug (EX: pro)</param>
        /// <param name="fileType">File Type</param>
        /// <param name="filter">Filter mode</param>
        /// <param name="seqn">Selected Seqn</param>
        [HttpGet("{code}/{fileType}"), ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(string code, FileType fileType = FileType.Help, [FromQuery] FileFilter? filter = default, [FromQuery] int? seqn = null)
        {
            var scheme = Request.Scheme;
            if (Request.Host.Host.Contains("blizztrack", StringComparison.OrdinalIgnoreCase))
            {
                scheme = "https";
            }

            var file = fileType.ToString().ToLower();
            AbstractCommand cmd;
            code = code.ToLower();
            object result;

            var parent = await _gameParents.Get(code);

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
                            name = config?.GetName() ?? data.Name,
                            data.Indexed,
                            data.Seqn,
                            code,
                            encrypted = config?.Config.Encrypted,
                            logos = parent?.Logos,
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
                            name = config?.GetName() ?? data.Name,
                            data.Indexed,
                            data.Seqn,
                            code,
                            encrypted = config?.Config.Encrypted,
                            logos = parent?.Logos,
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
                            name = config?.GetName() ?? data.Name,
                            data.Indexed,
                            data.Seqn,
                            code,
                            encrypted = config?.Config.Encrypted,
                            logos = parent?.Logos,
                            command = cmd.ToString(),
                            data = ver
                        };
                        break;
                    }
                case "seqn":
                    {
                        object data;
                        switch (filter?.ToString().ToLower())
                        {
                            case "version" or "versions":
                                {
                                    var ver = await _versions.SeqnList(code);
                                    if (ver == null) return NotFound(new { error = "Not found" });
                                    var config = await _gameConfig.Get(code);

                                    var f = ver.Select(seqnType => new
                                    {
                                        seqnType.Seqn,
                                        seqnType.Indexed,
                                        view = Url.Action("Get", "ngpd", new { code, file_type = "versions", seqn = seqnType.Seqn }, scheme),
                                    }).ToList();

                                    data = new
                                    {
                                        name = config?.GetName() ?? BNetLib.Helpers.GameName.Get(code),
                                        code = code.ToLower(),
                                        encrypted = config?.Config.Encrypted,
                                        logos = parent?.Logos,
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

                                    var f = cdn.Select(seqnType => new
                                    {
                                        seqnType.Seqn,
                                        seqnType.Indexed,
                                        view = Url.Action("Get", "ngpd", new { code, file_type = "cdn", seqn = seqnType.Seqn }, scheme),
                                    }).ToList();

                                    data = new
                                    {
                                        name = config?.GetName() ?? BNetLib.Helpers.GameName.Get(code),
                                        code = code.ToLower(),
                                        encrypted = config?.Config.Encrypted,
                                        logos = parent?.Logos,
                                        latest = f.First(),
                                        data = f
                                    };
                                    break;
                                }
                            case "bgdl":
                                {
                                    var bgdl = await _bgdl.SeqnList(code);
                                    if (bgdl == null)
                                        return NotFound(new { error = "Not found" });
                                    var config = await _gameConfig.Get(code);

                                    var f = bgdl.Select(seqnType => new
                                    {
                                        seqnType.Seqn,
                                        seqnType.Indexed,
                                        view = Url.Action("Get", "ngpd", new { code, file_type = "bdgl", seqn = seqnType.Seqn }, scheme),
                                    }).ToList();

                                    data = new
                                    {
                                        name = BNetLib.Helpers.GameName.Get(code),
                                        code = code.ToLower(),
                                        encrypted = config?.Config.Encrypted,
                                        logos = parent?.Logos,
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
                                        versions = Url.Action("Get", "ngpd", new { code, file_type = "seqn", filter = "versions" }, scheme),
                                        cdns = Url.Action("Get", "ngpd", new { code, file_type = "seqn", filter = "cdn" }, scheme),
                                        bgdl = Url.Action("Get", "ngpd", new { code, file_type = "seqn", filter = "bgdl" }, scheme),
                                    }
                                });
                        }

                        result = data;
                        break;
                    }
                default:
                    result = new
                    {
                        versions = Url.Action("Get", "ngpd", new { code, file_type = "version" }, scheme),
                        cdns = Url.Action("Get", "ngpd", new { code, file_type = "cdn" }, scheme),
                        bgdl = Url.Action("Get", "ngpd", new { code, file_type = "bgdl" }, scheme),
                        seqn = new
                        {
                            versions = Url.Action("Get", "ngpd", new { code, file_type = "seqn", filter = "versions" }, scheme),
                            cdns = Url.Action("Get", "ngpd", new { code, file_type = "seqn", filter = "cdn" }, scheme),
                            bgdl = Url.Action("Get", "ngpd", new { code, file_type = "seqn", filter = "bgdl" }, scheme),
                        },
                        supported = Url.Action("Get", "ngpd", new { code, file_type = "help" }, scheme),
                    };
                    break;
            }

            return Ok(result);
        }
    }
}