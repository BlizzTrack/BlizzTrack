using BNetLib.Networking.Commands;
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
    [Route("api/summary")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Summary Data")]
    [Produces("application/json")]
    public class SummaryController : ControllerBase
    {
        private readonly ISummary _summary;
        private readonly ILogger<SummaryController> _logger;
        private readonly IGameConfig _gameConfig;

        public SummaryController(ISummary summary, ILogger<SummaryController> logger, IGameConfig gameConfig)
        {
            _summary = summary;
            _logger = logger;
            _gameConfig = gameConfig;
        }

        /// <summary>
        ///     Changes between current and last summary
        /// </summary>
        /// <returns>Changes between current and last summary</returns>
        /// <response code="200">Returns differences between two summaries</response>
        [HttpGet("summary/changes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
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
                    SummaryDiff.Add(new
                    {
                        name = x.GetName(),
                        x.Product,
                        x.Flags,
                        x.Seqn,
                        encrypted = configs.FirstOrDefault(f => f.Code == x.Product)?.Config.Encrypted,
                        logos = configs.FirstOrDefault(f => f.Code == x.Product)?.Logos,
                        relations = new
                        {
                            view = Url.Action("Get", "ngpd", new { code = x.Product, file_type = x.Flags, seqn = x.Seqn }, scheme),
                            seqn = Url.Action("Get", "ngpd", new { code = x.Product, file_type = "seqn", filter = x.Flags }, scheme),
                        }
                    });
                }
            }

            return Ok(new
            {
                changes = SummaryDiff,
                latest = new
                {
                    latest.Code,
                    latest.Name,
                    latest.Seqn,
                    latest.Indexed,
                    content = latest.Content.Select(x => new
                    {
                        name = x.GetName(),
                        x.Product,
                        x.Flags,
                        x.Seqn,
                        encrypted = configs.FirstOrDefault(f => f.Code == x.Product)?.Config.Encrypted,
                        logos = configs.FirstOrDefault(f => f.Code == x.Product)?.Logos,
                        relations = new
                        {
                            view = Url.Action("Get", "ngpd", new { code = x.Product, file_type = x.Flags, seqn = x.Seqn }, scheme),
                            seqn = Url.Action("Get", "ngpd", new { code = x.Product, file_type = "seqn", filter = x.Flags }, scheme),
                        }
                    }).ToList()
                },
                previous = new
                {
                    previous.Code,
                    previous.Name,
                    previous.Seqn,
                    previous.Indexed,
                    content = previous.Content.Select(x => new
                    {
                        name = x.GetName(),
                        x.Product,
                        x.Flags,
                        x.Seqn,
                        encrypted = configs.FirstOrDefault(f => f.Code == x.Product)?.Config.Encrypted,
                        logos = configs.FirstOrDefault(f => f.Code == x.Product)?.Logos,
                        relations = new
                        {
                            view = Url.Action("Get", "ngpd", new { code = x.Product, file_type = x.Flags, seqn = x.Seqn }, scheme),
                            seqn = Url.Action("Get", "ngpd", new { code = x.Product, file_type = "seqn", filter = x.Flags }, scheme),
                        }
                    }).ToList()
                },
            });
        }
        /// <summary>
        ///     Returns latest summary
        /// </summary>
        /// <returns>Returns latest summary data</returns>
        /// <response code="200">Returns latest summary data</response>
        /// <param name="filter">Filter mode</param>
        /// <param name="seqn">Selected Seqn</param>
        /// <param name="game_filter">Game code filter (EX: pro)</param>
        [HttpGet("{filter?}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Summary(SummaryFilter? filter = SummaryFilter.All, [FromQuery] int? seqn = null, [FromQuery] string game_filter = default)
        {
            object result = null;

            switch (filter)
            {
                case SummaryFilter.Seqn:
                    {
                        var summary = await _summary.Seqn();
                        if (summary == null) return NotFound(new { error = "Not found" });

                        var f = summary.Select(data => new
                        {
                            data.Seqn,
                            data.Indexed,
                            view = Url.Action("Summary", "ngpd", new { file = SummaryFilter.All, seqn = data.Seqn }, HttpContext.Request.Scheme),
                        }).ToList();

                        result = new
                        {
                            name = "Summary",
                            code = "Summary",
                            latest = f.First(),
                            data = f
                        };
                        break;
                    }
                case SummaryFilter.All:
                default:
                    {
                        var data = await _summary.Single(seqn);
                        if (data == null) return NotFound(new { error = "Not Found" });
                        var configs = await _gameConfig.In(data.Content.Select(x => x.Product).ToArray());

                        result = new
                        {
                            data.Seqn,
                            command = new SummaryCommand().ToString(),
                            name = "summary",
                            code = "summary",
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
                                    view = Url.Action("Get", "ngpd", new { code = x.Product, file_type = x.Flags, seqn = x.Seqn }, HttpContext.Request.Scheme),
                                    seqn = Url.Action("Get", "ngpd", new { code = x.Product, file_type = "seqn", filter = x.Flags }, HttpContext.Request.Scheme),
                                }
                            }).ToList().Where(x => game_filter == default || x.Product.Contains(game_filter?.ToString(), StringComparison.OrdinalIgnoreCase))
                        };
                        break;
                    }
            }

            return Ok(result);
        }
    }
}
