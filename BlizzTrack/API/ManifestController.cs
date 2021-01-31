using BlizzTrack.Models;
using BNetLib.Networking.Commands;
using Core.Models;
using Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace BlizzTrack.API
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Game Versions")]
    [Produces("application/json")]
    public class ManifestController : ControllerBase
    {
        private readonly IVersions _versions;
        private readonly ICDNs _cdns;
        private readonly IBGDL _bgdl;
        private readonly ISummary _summary;
        private readonly ILogger<ManifestController> _logger;
        private readonly IGameConfig _gameConfig;
        private readonly IGameParents _gameParents;

        public ManifestController(IVersions versions, ICDNs cdns, IBGDL bgdl, ILogger<ManifestController> logger, IGameConfig gameConfig, ISummary summary, IGameParents gameParents)
        {
            _versions = versions;
            _cdns = cdns;
            _bgdl = bgdl;
            _logger = logger;
            _gameConfig = gameConfig;
            _summary = summary;
            _gameParents = gameParents;
        }

        /// <summary>
        ///     List all manifest
        /// </summary>
        /// <returns>Returns list of all game manifest API calls</returns>
        /// <response code="200">All API manifest relations</response>
        [HttpGet("")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Results.Help>))]
        public async Task<IActionResult> List()
        {
            var games = await _summary.Latest();
            var gameTypes = games.Content.Where(x => x.Flags != "cdns").ToList();

            var res = gameTypes.GroupBy(x => x.Product).Select(x => x.First()).Select(x => new Results.Help()
            {
                Versions = Url.Action("Versions", "Manifest", new { code = x.Product }, HttpContext.Request.Scheme),
                CDNs = Url.Action("CDNs", "Manifest", new { code = x.Product }, HttpContext.Request.Scheme),
                BGDL = Url.Action("BGDL", "Manifest", new { code = x.Product }, HttpContext.Request.Scheme),
                Seqn = new Dictionary<Results.SeqnType, string>()
                {
                    { 
                        Results.SeqnType.Versions, 
                        Url.Action("Seqn", "Manifest", new {
                            code = x.Product, 
                            filter = Results.SeqnType.Versions
                        }, HttpContext.Request.Scheme)  
                    },
                    {
                        Results.SeqnType.CDNs,
                        Url.Action("Seqn", "Manifest", new {
                            code = x.Product, 
                            filter = Results.SeqnType.CDNs
                        }, HttpContext.Request.Scheme)
                    },
                    {
                        Results.SeqnType.BGDL,
                        Url.Action("Seqn", "Manifest", new {
                            code = x.Product, 
                            filter = Results.SeqnType.BGDL
                        }, HttpContext.Request.Scheme)
                    }
                }
            }).ToList();

            return Ok(res);
        }

        /// <summary>
        ///     List all seqn for game
        /// </summary>
        /// <returns>Returns list of all game seqn for file</returns>
        /// <response code="200">All API manifest relations</response>
        /// <param name="code">Selected game code</param>
        /// <param name="filter">Selected file type</param>
        [HttpGet("seqn/{code}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Results.Result<List<SharedResults.SeqnItem>>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ReponseTypes.NotFound))]
        public async Task<IActionResult> Seqn(string code, [Required]Results.SeqnType filter)
        {
            var res = new Results.Result<List<SharedResults.SeqnItem>>();
            var config = await _gameConfig.Get(code);
            if(config == null || config.Code != code)
            {
                return NotFound(new ReponseTypes.NotFound());
            }
            var parent = await _gameParents.Get(code);

            res.Product = code.ToLower();
            res.Name = BNetLib.Helpers.GameName.Get(code);
            res.Logos = parent.Logos;
            res.Encrypted = config.Config.Encrypted;
            switch(filter)
            {
                case Results.SeqnType.Versions:
                    {
                        var seqns = await _versions.Seqn(code);
                        res.Data = seqns.Select(x => new SharedResults.SeqnItem
                        {
                            Seqn = x.Seqn,
                            Indexed = x.Indexed,
                            View = Url.Action("Versions", "Manifest", new
                            {
                                code = x.Code,
                                seqn = x.Seqn
                            }, HttpContext.Request.Scheme)
                        }).ToList();
                    }
                    break;
                case Results.SeqnType.BGDL:
                    {
                        var seqns = await _bgdl.Seqn(code);
                        res.Data = seqns.Select(x => new SharedResults.SeqnItem
                        {
                            Seqn = x.Seqn,
                            Indexed = x.Indexed,
                            View = Url.Action("BGDL", "Manifest", new
                            {
                                code = x.Code,
                                seqn = x.Seqn
                            }, HttpContext.Request.Scheme)
                        }).ToList();
                    }
                    break;
                case Results.SeqnType.CDNs:
                    {
                        var seqns = await _cdns.Seqn(code);
                        res.Data = seqns.Select(x => new SharedResults.SeqnItem
                        {
                            Seqn = x.Seqn,
                            Indexed = x.Indexed,
                            View = Url.Action("CDNs", "Manifest", new
                            {
                                code = x.Code,
                                seqn = x.Seqn
                            }, HttpContext.Request.Scheme)
                        }).ToList();
                    }
                    break;
            }

            return Ok(res);
        }

        /// <summary>
        ///     Version File Data
        /// </summary>
        /// <returns>Returns Versions data for selected code and seqn</returns>
        /// <response code="200">Versions file data</response>
        /// <param name="code">Selected game code</param>
        /// <param name="seqn">Seqn to view</param>
        [HttpGet("versions/{code}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Results.Result<List<Results.Versions>>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ReponseTypes.NotFound))]
        public async Task<IActionResult> Versions(string code, int? seqn)
        {
            var res = new Results.Result<List<Results.Versions>>();

            var config = await _gameConfig.Get(code);
            if (config == null || config.Code != code) return NotFound(new ReponseTypes.NotFound());
  
            var parent = await _gameParents.Get(code);
            var versions = await _versions.Single(code, seqn);
            if(versions == null) return NotFound(new ReponseTypes.NotFound());

            res.Data = versions.Content.Select(x => new Results.Versions()
            {
                Buildconfig = x.Buildconfig,
                Buildid = x.Buildid,
                Cdnconfig = x.Cdnconfig,
                Keyring = x.Keyring,
                Region = x.Region,
                Versionsname = x.Versionsname,
                Productconfig = x.Productconfig,
                RegionName = x.GetName()
            }).ToList();

            res.Product = code.ToLower();
            res.Name = BNetLib.Helpers.GameName.Get(code);
            res.Logos = parent.Logos;
            res.Encrypted = config.Config.Encrypted;
            res.Command = new VersionCommand(code).ToString();
            res.Seqn = versions.Seqn;
            res.File = "verisons";
            res.Indexed = versions.Indexed;

            var relations = new Dictionary<SharedResults.RelationTypes, string>();
            if (parent.PatchNoteAreas != null && parent.PatchNoteAreas.Count > 0)
            {
                relations[SharedResults.RelationTypes.PatchNotes] = Url.Action("List", "PatchNotes",
                    new { game = parent.Slug },
                    HttpContext.Request.Scheme
                );
            }

            if (relations.Count > 0)
            {
                res.Relations = relations;
            }

            return Ok(res);
        }

        /// <summary>
        ///     BGDL File Data
        /// </summary>
        /// <returns>Returns BGDL data for selected code and seqn</returns>
        /// <response code="200">BGDL file data</response>
        /// <param name="code">Selected game code</param>
        /// <param name="seqn">Seqn to view</param>
        [HttpGet("bgdl/{code}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Results.Result<List<Results.Versions>>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ReponseTypes.NotFound))]
        public async Task<IActionResult> BGDL(string code, int? seqn)
        {
            var res = new Results.Result<List<Results.Versions>>();

            var config = await _gameConfig.Get(code);
            if (config == null || config.Code != code) return NotFound(new ReponseTypes.NotFound());

            var parent = await _gameParents.Get(code);
            var versions = await _bgdl.Single(code, seqn);
            if (versions == null) return NotFound(new ReponseTypes.NotFound());

            res.Data = versions.Content.Select(x => new Results.Versions()
            {
                Buildconfig = x.Buildconfig,
                Buildid = x.Buildid,
                Cdnconfig = x.Cdnconfig,
                Keyring = x.Keyring,
                Region = x.Region,
                Versionsname = x.Versionsname,
                Productconfig = x.Productconfig,
                RegionName = x.GetName()
            }).ToList();

            res.Product = code.ToLower();
            res.Name = BNetLib.Helpers.GameName.Get(code);
            res.Logos = parent.Logos;
            res.Encrypted = config.Config.Encrypted;
            res.Command = new BGDLCommand(code).ToString();
            res.Seqn = versions.Seqn;
            res.File = "bgdl";
            res.Indexed = versions.Indexed;

            var relations = new Dictionary<SharedResults.RelationTypes, string>();
            if (parent.PatchNoteAreas != null && parent.PatchNoteAreas.Count > 0)
            {
                relations[SharedResults.RelationTypes.PatchNotes] = Url.Action("List", "PatchNotes",
                    new { game = parent.Slug },
                    HttpContext.Request.Scheme
                );
            }

            if (relations.Count > 0)
            {
                res.Relations = relations;
            }

            return Ok(res);
        }

        /// <summary>
        ///     CDNs File Data
        /// </summary>
        /// <returns>Returns CDNs data for selected code and seqn</returns>
        /// <response code="200">CDN file data</response>
        /// <param name="code">Selected game code</param>
        /// <param name="seqn">Seqn to view</param>
        [HttpGet("cdns/{code}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Results.Result<List<Results.CDNs>>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ReponseTypes.NotFound))]
        public async Task<IActionResult> CDNs(string code, int? seqn)
        {
            var res = new Results.Result<List<Results.CDNs>>();

            var config = await _gameConfig.Get(code);
            if (config == null || config.Code != code) return NotFound(new ReponseTypes.NotFound());

            var parent = await _gameParents.Get(code);
            var versions = await _cdns.Single(code, seqn);
            if (versions == null) return NotFound(new ReponseTypes.NotFound());

            res.Data = versions.Content.Select(x => new Results.CDNs()
            {
                Region = x.Name,
                Path = x.Path,
                Hosts = x.Hosts.Split(" "),
                Servers = x.Servers.Split(" "),
                ConfigPath = x.ConfigPath,
                RegionName = x.GetName()
            }).ToList();

            res.Product = code.ToLower();
            res.Name = BNetLib.Helpers.GameName.Get(code);
            res.Logos = parent.Logos;
            res.Encrypted = config.Config.Encrypted;
            res.Command = new CDNCommand(code).ToString();
            res.Seqn = versions.Seqn;
            res.File = "cdn";
            res.Indexed = versions.Indexed;

            var relations = new Dictionary<SharedResults.RelationTypes, string>();
            if(parent.PatchNoteAreas != null && parent.PatchNoteAreas.Count > 0)
            {
                relations[SharedResults.RelationTypes.PatchNotes] = Url.Action("List", "PatchNotes",
                    new { game = parent.Slug },
                    HttpContext.Request.Scheme
                );
            }

            if(relations.Count > 0)
            {
                res.Relations = relations;
            }

            return Ok(res);
        }

        public class Results
        {
            [DataContract]
            [JsonConverter(typeof(StringEnumConverter))]
            public enum SeqnType
            {
                Versions,
                CDNs,
                BGDL
            }

            public class Result<T>
            {
                /// <summary>
                ///     Game Code
                /// </summary>
                public string Product { get; set; }

                /// <summary>
                ///     File Type
                /// </summary>
                public string File { get; set; }

                /// <summary>
                ///     NGPD Command
                /// </summary>
                public string Command { get; set; } = null;

                /// <summary>
                ///     File Seqn
                /// </summary>
                public int? Seqn { get; set; } = null;

                /// <summary>
                ///     Time Indexed
                /// </summary>
                public DateTime? Indexed { get; set; } = null;

                /// <summary>
                ///     If this game is encrypted
                /// </summary>
                public bool? Encrypted { get; set; }

                /// <summary>
                ///     Game logos/assets
                /// </summary>
                public List<Icons> Logos { get; set; }

                /// <summary>
                ///     Relations to other pages
                /// </summary>
                public Dictionary<SharedResults.RelationTypes, string> Relations { get; set; } = null;
  
                /// <summary>
                ///     Game Name
                /// </summary>
                public string Name { get; set; }

                public T Data { get; set; }
            }

            public class Help
            {
                public string Versions { get; set; }

                [JsonProperty("cdns")]
                public string CDNs { get; set; }

                [JsonProperty("bgdl")]
                public string BGDL { get; set; }

                public Dictionary<SeqnType, string> Seqn { get; set; }
            }

            public class Versions
            {
                [JsonProperty("region_name")]
                public string RegionName { get; set; }

                public string Buildconfig { get; set; }

                public int Buildid { get; set; }
                
                public string Cdnconfig { get; set; }
                
                public string Keyring { get; set; }
                
                public string Region { get; set; }
                
                public string Versionsname { get; set; }
                
                public string Productconfig { get; set; }
            }

            public class CDNs
            {
                [JsonProperty("region_name")]
                public string RegionName { get; set; }

                public string Region { get; set; }

                public string Path { get; set; }

                public string[] Servers { get; set; }

                public string[] Hosts { get; set; }

                public string ConfigPath { get; set; }
            }
        }
    }
}
