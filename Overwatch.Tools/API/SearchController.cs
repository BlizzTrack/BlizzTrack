using Core.Attributes;
using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Overwatch.Tools.Models;
using Overwatch.Tools.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Overwatch.Tools.API
{
    [GameToolRoute(typeof(SearchController), "[Controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Overwatch Game Data")]
    [Produces("application/json")]
    public class SearchController : ControllerBase
    {
        private readonly IOverwatchSearchService _overwatchSearchService;
        private readonly DBContext _dbContext;

        public SearchController(IOverwatchSearchService overwatchSearchService, DBContext dbContext)
        {
            _overwatchSearchService = overwatchSearchService;
            _dbContext = dbContext;
        }

        /// <summary>
        ///     Search players on Overwatch
        /// </summary>
        /// <returns>Returns a list of known players by this battle tag</returns>
        /// <response code="200">Returns latest of players with this battle tag</response>
        /// <param name="player">User name of the person</param>
        /// <param name="searchMode">Mode you wish to search with</param>
        /// <param name="platformMode">The platform you wish to search on</param>
        [HttpGet("{player}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<PlayerSearchResult<Portrait>>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ReponseTypes.NotFound))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ReponseTypes.BadRequest))]
        public async Task<IActionResult> SearchPlayers(
            string player, 
            [FromQuery(Name = "search_mode")]SearchMode? searchMode = null,
            [FromQuery(Name = "platform")]SearchPlatform? platformMode = null)
        {
            var res = await _overwatchSearchService.Search(player);

            if (platformMode != null)
            {
                var pMode = platformMode.Value.ToString();
                if (platformMode.Value == SearchPlatform.NINTENDOSWITCH)
                {
                    pMode = "nintendo-switch";
                }

                res = res.Where(x => x.Platform.Equals(pMode, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (searchMode != null)
            {
                res = searchMode switch
                {
                    SearchMode.LevelHighToLow => res.OrderByDescending(x => x.Level).ToList(),
                    SearchMode.LevelLowToHigh => res.OrderBy(x => x.Level).ToList(),
                    _ => res
                };
            }

            var ids = res.Select(x => x.Portrait).ToList();
            var assets = await _dbContext.Assets
                .Where(x => ids.Contains(x.Metadata.RootElement.GetProperty("id").GetString())).ToListAsync();
            
            return Ok(res.Select(x =>
            {
                var asset = assets.FirstOrDefault(z =>
                    z.Metadata.RootElement.GetProperty("id").GetString() == x.Portrait);
                
                return new PlayerSearchResult<Portrait>()
                {
                    Id = x.Id,
                    Level = x.Level,
                    Name = x.Name,
                    Platform =  x.Platform,
                    IsPublic =  x.IsPublic,
                    UrlName =  x.UrlName,
                    Portrait = asset == null ? null : new Portrait()
                    {
                        Id = asset.Metadata.RootElement.GetProperty("id").GetString(),
                        Url = asset.url,
                        Name = asset.Metadata.RootElement.GetProperty("name").GetString(),
                        Version = asset.Metadata.RootElement.GetProperty("release").GetString(),
                        Event = asset.Metadata.RootElement.GetProperty("event").GetString()
                    }
                };
            }));
        }
    }
}
