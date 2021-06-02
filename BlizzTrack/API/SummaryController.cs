using BlizzTrack.Models;
using BNetLib.Networking.Commands;
using Core.Models;
using Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlizzTrack.API
{
    [Route("api/summary"), ApiController, ApiExplorerSettings(GroupName = "Summary Data"), Produces("application/json")]
    public class SummaryController : ControllerBase
    {
        private readonly ISummary _summary;
        private readonly ILogger<SummaryController> _logger;
        private readonly IGameConfig _gameConfig;
        private readonly IGameParents _gameParents;

        public SummaryController(ISummary summary, ILogger<SummaryController> logger, IGameConfig gameConfig, IGameParents gameParents)
        {
            _summary = summary;
            _logger = logger;
            _gameConfig = gameConfig;
            _gameParents = gameParents;
        }

        /// <summary>
        ///     Changes between current and last summary
        /// </summary>
        /// <returns>Changes between current and last summary</returns>
        /// <response code="200">Returns differences between two summaries</response>
        [HttpGet("changes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces(typeof(SummaryResults.SummaryChanges))]
        public async Task<IActionResult> GetChanges()
        {
            var manifests = await _summary.Take(2);

            var latest = manifests.First();
            var previous = manifests.Last();

            var summaryDiff = new List<SummaryResults.SummaryItem>();
            var configs = await _gameConfig.In(latest.Content.Select(x => x.Product).ToArray());
            var parents = await _gameParents.All();
            
            foreach (var item in latest.Content)
            {
                var config = configs.FirstOrDefault(f => f.Code == item.Product);
                var parent = parents.FirstOrDefault(x =>
                    item.Product.StartsWith(x.Code) || x.ChildrenOverride.Contains(item.Product));

                var x = previous.Content.FirstOrDefault(x => x.Product == item.Product && x.Flags == item.Flags);
                if (x == null || x.Seqn != item.Seqn)
                {
                    summaryDiff.Add(new SummaryResults.SummaryItem
                    {
                        Name = string.IsNullOrEmpty(config?.Name) ? item.GetName() : config.Name,
                        Product = x.Product,
                        Flags = x.Flags,
                        Seqn = x.Seqn,
                        Encrypted = config?.Config.Encrypted,
                        Logos = parent?.Logos,
                        Relations = new Dictionary<SharedResults.RelationTypes, string>()
                        {
                            {
                                SharedResults.RelationTypes.View,
                                Url.Action(x.Flags == "cdn" ? "cdns" : x.Flags, "Manifest", new { code = x.Product, seqn = x.Seqn }, Scheme())
                            },
                            {
                                SharedResults.RelationTypes.Seqn,
                                Url.Action("Seqn", "Manifest", new { code = x.Product, filter = x.Flags == "cdn" ? "cdns" : x.Flags }, Scheme())
                            }
                        }
                    });
                }
            }

            return Ok(new SummaryResults.SummaryChanges
            {
                Changes = summaryDiff,
                Latest = new SummaryResults.SummaryChange
                {
                    Code = latest.Code,
                    Name = latest.Name,
                    Seqn = latest.Seqn,
                    Indexed = latest.Indexed,
                    Data = latest.Content.Select(x => {
                        var config = configs.FirstOrDefault(f => f.Code == x.Product);
                        var parent = parents.FirstOrDefault(z =>
                            x.Product.StartsWith(z.Code) || z.ChildrenOverride.Contains(x.Product));

                        return new SummaryResults.SummaryItem
                        {
                            Name = string.IsNullOrEmpty(config?.Name) ? x.GetName() : config.Name,
                            Product = x.Product,
                            Flags = x.Flags,
                            Seqn = x.Seqn,
                            Encrypted = config?.Config.Encrypted,
                            Logos = parent?.Logos,
                            Relations = new Dictionary<SharedResults.RelationTypes, string>()
                        {
                            {
                                SharedResults.RelationTypes.View,
                                Url.Action(x.Flags == "cdn" ? "cdns" : x.Flags, "Manifest", new { code = x.Product, seqn = x.Seqn }, Scheme())
                            },
                            {
                                SharedResults.RelationTypes.Seqn,
                                Url.Action("Seqn", "Manifest", new { code = x.Product, filter = x.Flags == "cdn" ? "cdns" : x.Flags }, Scheme())
                            }
                        }
                        };
                    }).ToList()
                },
                Previous = new SummaryResults.SummaryChange
                {
                    Code = previous.Code,
                    Name = previous.Name,
                    Seqn = previous.Seqn,
                    Indexed = previous.Indexed,
                    Data = previous.Content.Select(x => {
                        var config = configs.FirstOrDefault(f => f.Code == x.Product);
                        var parent = parents.FirstOrDefault(z =>
                            x.Product.StartsWith(z.Code) || z.ChildrenOverride.Contains(x.Product));

                        return new SummaryResults.SummaryItem
                        {
                            Name = string.IsNullOrEmpty(config?.Name) ? x.GetName() : config.Name,
                            Product = x.Product,
                            Flags = x.Flags,
                            Seqn = x.Seqn,
                            Encrypted = config?.Config.Encrypted,
                            Logos = parent?.Logos,
                            Relations = new Dictionary<SharedResults.RelationTypes, string>()
                        {
                            {
                                SharedResults.RelationTypes.View,
                                Url.Action(x.Flags == "cdn" ? "cdns" : x.Flags, "Manifest", new { code = x.Product, seqn = x.Seqn }, Scheme())
                            },
                            {
                                SharedResults.RelationTypes.Seqn,
                                Url.Action("Seqn", "Manifest", new { code = x.Product, filter = x.Flags == "cdn" ? "cdns" : x.Flags }, Scheme())
                            }
                        }
                        };
                    }).ToList()
                },
            });
        }

        /// <summary>
        ///     Returns list of seqn's in given summary
        /// </summary>
        /// <returns>Returns latest summary data</returns>
        /// <response code="200">Returns latest summary seqn list</response>
        [HttpGet("seqn"),
         ProducesResponseType(StatusCodes.Status200OK,
             Type = typeof(SummaryResults.ResultBase<List<SharedResults.SeqnItem>>)),
         ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ReponseTypes.NotFound))]
        public async Task<IActionResult> SummarySeqn()
        {
            var summary = await _summary.Seqn();
            if (summary == null) return NotFound(new ReponseTypes.NotFound());

            var f = summary.Select(data => new SharedResults.SeqnItem
            {
                Seqn = data.Seqn,
                Indexed = data.Indexed,
                View = Url.Action("SummaryList", "Summary", new { seqn = data.Seqn }, Scheme()),
            }).ToList();

            var result = new SummaryResults.ResultBase<List<SharedResults.SeqnItem>>
            {
                Name = "Summary",
                Code = "Summary",
                Data = f
            };

            return Ok(result);
        }
        /// <summary>
        ///     Returns summary data for given seqn
        /// </summary>
        /// <returns>Returns summary data for given seqn (latest if empty)</returns>
        /// <response code="200">Returns summary data</response>
        /// <param name="seqn">Selected Seqn</param>
        /// <param name="gameFilter">Game code filter (EX: pro)</param>
        [HttpGet(""), HttpGet("all"),
         ProducesResponseType(StatusCodes.Status200OK,
             Type = typeof(SummaryResults.ResultBase<List<SummaryResults.SummaryItem>>)),
         ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ReponseTypes.NotFound))]
        public async Task<IActionResult> SummaryList([FromQuery] int? seqn, [FromQuery(Name = "game_filter")] string gameFilter)
        {
            var data = await _summary.Single(seqn);
            if (data == null) return NotFound(new ReponseTypes.NotFound());
            var configs = await _gameConfig.In(data.Content.Select(x => x.Product).ToArray());
            var parents = await _gameParents.All();
            
            var result = new SummaryResults.ResultBase<List<SummaryResults.SummaryItem>>()
            {
                Seqn = data.Seqn,
                Command = new SummaryCommand().ToString(),
                Name = "summary",
                Code = "summary",
                Data = data.Content.Select(x => {
                    var config = configs.FirstOrDefault(f => f.Code == x.Product);
                    var parent = parents.FirstOrDefault(z =>
                        x.Product.StartsWith(z.Code) || z.ChildrenOverride.Contains(x.Product));
                    return new SummaryResults.SummaryItem
                    {
                        Name = string.IsNullOrEmpty(config?.Name) ? x.GetName() : config.Name,
                        Product = x.Product,
                        Flags = x.Flags,
                        Seqn = x.Seqn,
                        Encrypted = config?.Config.Encrypted,
                        Logos = parent?.Logos,
                        Relations = new Dictionary<SharedResults.RelationTypes, string>
                        {
                            {
                                SharedResults.RelationTypes.View,
                                Url.Action(x.Flags == "cdn" ? "cdns" : x.Flags, "Manifest", new { code = x.Product, seqn = x.Seqn }, Scheme())
                            },
                            {
                                SharedResults.RelationTypes.Seqn,
                                Url.Action("Seqn", "Manifest", new { code = x.Product, filter = x.Flags == "cdn" ? "cdns" : x.Flags }, Scheme())
                            }
                        }
                    };
                }).Where(x => gameFilter == default || x.Product.Contains(gameFilter, StringComparison.OrdinalIgnoreCase)).ToList()
            };

            return Ok(result);
        }

        private string Scheme()
        {
            return HttpContext.Request.Host.Host.Contains("blizztrack", StringComparison.OrdinalIgnoreCase) ? "https" : "http";
        }
    }

    public class SummaryResults
    {
        public class ResultBase<T>
        {
            /// <summary>
            ///     File Seqn
            /// </summary>
            public int? Seqn { get; set; }

            /// <summary>
            ///     NGPD Command Sent
            /// </summary>
            public string Command { get; set; }

            /// <summary>
            ///     Game Name
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            ///     Game Code   
            /// </summary>
            public string Code { get; set; }

            /// <summary>
            ///     Request Payload
            /// </summary>
            public T Data { get; set; }
        }

        public class SummaryChanges
        {
            /// <summary>
            ///     Difference between latest and previous
            /// </summary>
            public List<SummaryItem> Changes { get; set; }

            /// <summary>
            ///     Current Seqn
            /// </summary>
            public SummaryChange Latest { get; set; }

            /// <summary>
            ///     Previous Seqn
            /// </summary>
            public SummaryChange Previous { get; set; }
        }


        public class SummaryChange : ResultBase<List<SummaryItem>>
        {
            /// <summary>
            ///     Date indexed
            /// </summary>
            public DateTime Indexed { get; set; }
        }

        public class SummaryItem
        {
            /// <summary>
            ///     Game Code
            /// </summary>
            public string Product { get; set; }

            /// <summary>
            ///     File Type
            /// </summary>
            public string Flags { get; set; }

            /// <summary>
            ///     File Seqn
            /// </summary>
            public int Seqn { get; set; }

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
            public Dictionary<SharedResults.RelationTypes, string> Relations { get; set; }

            /// <summary>
            ///     Game Name
            /// </summary>
            public string Name { get; set; }
        }
    }
}
