using Core.Attributes;
using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Overwatch.Tools.Models;
using Overwatch.Tools.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Overwatch.Tools.API
{
    [GameToolRoute(typeof(SearchController), "[Controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Overwatch Game Data")]
    [Produces("application/json")]
    public class SearchController : ControllerBase
    {
        private readonly IOverwatchSearchService _overwatchSearchService;

        public SearchController(IOverwatchSearchService overwatchSearchService)
        {
            _overwatchSearchService = overwatchSearchService;
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<PlayerSearchResult>))]
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
                switch (searchMode)
                {
                    case SearchMode.LevelHighToLow:
                        res = res.OrderByDescending(x => x.Level).ToList();
                        break;
                    case SearchMode.LevelLowToHigh:
                        res = res.OrderBy(x => x.Level).ToList();
                        break;
                }

            }


            return Ok(res);
        }
    }
}
