﻿using BNetLib.Converters;
using BNetLib.Networking.Commands;
using Core.Models;
using Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
        [Produces(typeof(SummaryResults.SummaryChanges))]
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

            var SummaryDiff = new List<SummaryResults.SummaryItem>();
            var configs = await _gameConfig.In(latest.Content.Select(x => x.Product).ToArray());

            foreach (var item in latest.Content)
            {
                var x = previous.Content.FirstOrDefault(x => x.Product == item.Product && x.Flags == item.Flags);
                if (x == null || x.Seqn != item.Seqn)
                {
                    SummaryDiff.Add(new SummaryResults.SummaryItem()
                    {
                        Name = x.GetName(),
                        Product = x.Product,
                        Flags = x.Flags,
                        Seqn = x.Seqn,
                        Encrypted = configs.FirstOrDefault(f => f.Code == x.Product)?.Config.Encrypted,
                        Logos = configs.FirstOrDefault(f => f.Code == x.Product)?.Logos,
                        Relations = new
                        {
                            view = Url.Action("Get", "ngpd", new { code = x.Product, file_type = x.Flags, seqn = x.Seqn }, scheme),
                            seqn = Url.Action("Get", "ngpd", new { code = x.Product, file_type = "seqn", filter = x.Flags }, scheme),
                        }
                    });
                }
            }

            return Ok(new SummaryResults.SummaryChanges
            {
                Changes = SummaryDiff,
                Latest = new SummaryResults.SummaryChange
                { 
                    Code = latest.Code,
                    Name = latest.Name,
                    Seqn = latest.Seqn,
                    Indexed = latest.Indexed,
                    Data = latest.Content.Select(x => new SummaryResults.SummaryItem
                    {
                        Name = x.GetName(),
                        Product = x.Product,
                        Flags = x.Flags,
                        Seqn = x.Seqn,
                        Encrypted = configs.FirstOrDefault(f => f.Code == x.Product)?.Config.Encrypted,
                        Logos = configs.FirstOrDefault(f => f.Code == x.Product)?.Logos,
                        Relations = new
                        {
                            view = Url.Action("Get", "ngpd", new { code = x.Product, file_type = x.Flags, seqn = x.Seqn }, scheme),
                            seqn = Url.Action("Get", "ngpd", new { code = x.Product, file_type = "seqn", filter = x.Flags }, scheme),
                        }
                    }).ToList()
                },
                Previous = new SummaryResults.SummaryChange
                {
                    Code = previous.Code,
                    Name = previous.Name,
                    Seqn = previous.Seqn,
                    Indexed = previous.Indexed,
                    Data = previous.Content.Select(x => new SummaryResults.SummaryItem
                    {
                        Name = x.GetName(),
                        Product = x.Product,
                        Flags = x.Flags,
                        Seqn = x.Seqn,
                        Encrypted = configs.FirstOrDefault(f => f.Code == x.Product)?.Config.Encrypted,
                        Logos = configs.FirstOrDefault(f => f.Code == x.Product)?.Logos,
                        Relations = new
                        {
                            view = Url.Action("Get", "ngpd", new { code = x.Product, file_type = x.Flags, seqn = x.Seqn }, scheme),
                            seqn = Url.Action("Get", "ngpd", new { code = x.Product, file_type = "seqn", filter = x.Flags }, scheme),
                        }
                    }).ToList()
                },
            });
        }

        /// <summary>
        ///     Returns list of seqn's in given summary
        /// </summary>
        /// <returns>Returns latest summary data</returns>
        /// <response code="200">Returns latest summary seqn list</response>
        [HttpGet("seqn")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SummaryResults.ResultBase<List<SummaryResults.SeqnItem>>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ReponseTypes.NotFound))]
        public async Task<IActionResult> SummarySeqn()
        {
            var summary = await _summary.Seqn();
            if (summary == null) return NotFound(new ReponseTypes.NotFound());

            var f = summary.Select(data => new SummaryResults.SeqnItem
            {
                Seqn = data.Seqn,
                Indexed = data.Indexed,
                View = Url.Action("Summary", "ngpd", new { file = SummaryFilter.All, seqn = data.Seqn }, HttpContext.Request.Scheme),
            }).ToList();

            var result = new SummaryResults.ResultBase<List<SummaryResults.SeqnItem>>
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
        /// <param name="game_filter">Game code filter (EX: pro)</param>
        [HttpGet("")]
        [HttpGet("all")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SummaryResults.ResultBase<List<SummaryResults.SummaryItem>>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ReponseTypes.NotFound))]
        public async Task<IActionResult> SummaryList([FromQuery] int? seqn, [FromQuery] string game_filter)
        {
            var data = await _summary.Single(seqn);
            if (data == null) return NotFound(new ReponseTypes.NotFound());
            var configs = await _gameConfig.In(data.Content.Select(x => x.Product).ToArray());

            var result = new SummaryResults.ResultBase <List<SummaryResults.SummaryItem>>()
            {
                Seqn = data.Seqn,
                Command = new SummaryCommand().ToString(),
                Name = "summary",
                Code = "summary",
                Data = data.Content.Select(x => new SummaryResults.SummaryItem
                {
                    Name = x.GetName(),
                    Product = x.Product,
                    Flags = x.Flags,
                    Seqn = x.Seqn,
                    Encrypted = configs.FirstOrDefault(f => f.Code == x.Product)?.Config.Encrypted,
                    Logos = configs.FirstOrDefault(f => f.Code == x.Product)?.Logos,
                    Relations = new
                    {
                        view = Url.Action("Get", "ngpd", new { code = x.Product, file_type = x.Flags, seqn = x.Seqn }, HttpContext.Request.Scheme),
                        seqn = Url.Action("Get", "ngpd", new { code = x.Product, file_type = "seqn", filter = x.Flags }, HttpContext.Request.Scheme),
                    }
                }).Where(x => game_filter == default || x.Product.Contains(game_filter?.ToString(), StringComparison.OrdinalIgnoreCase)).ToList()
            };

            return Ok(result);
        }
    }

    public class SummaryResults
    {
        public class ResultBase<T>
        {
            /// <summary>
            ///     File Seqn
            /// </summary>
            public int? Seqn { get; set; } = null;

            /// <summary>
            ///     NGPD Command Sent
            /// </summary>
            public string Command { get; set; } = null;

            /// <summary>
            ///     Game Name
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            ///     Game Code
            /// </summary>
            public string Code { get; set; }

            public T Data { get; set; }
        }

        public class SummaryChanges
        {
            public List<SummaryItem> Changes { get; set; }

            public SummaryChange Latest { get; set; }

            public SummaryChange Previous { get; set; }
        }


        public class SummaryChange : ResultBase<List<SummaryItem>>
        {
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
            public object Relations { get; set; }

            /// <summary>
            ///     Game Name
            /// </summary>
            public string Name { get; set; }
        }

        public class SeqnItem
        {
            /// <summary>
            ///     Summary Seqn
            /// </summary>
            public int Seqn { get; set; }

            /// <summary>
            ///     Date indexed
            /// </summary>
            public DateTime Indexed { get; set; }

            /// <summary>
            ///     Link to view seqn
            /// </summary>
            public string View { get; set; }
        }
    }
}