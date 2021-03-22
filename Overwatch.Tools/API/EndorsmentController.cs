using Core.Attributes;
using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Overwatch.Tools.Models;
using Overwatch.Tools.Services;
using System.Threading.Tasks;

namespace Overwatch.Tools.API
{
    [GameToolRoute(typeof(EndorsementsController), "[Controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Overwatch Game Data")]
    [Produces("application/json")]
    public class EndorsementsController : ControllerBase
    {
        private readonly IOverwatchProfileService _overwatchProfileService;

        public EndorsementsController(IOverwatchProfileService overwatchProfileService)
        {
            this._overwatchProfileService = overwatchProfileService;
        }


        /// <summary>
        ///     Get players endorsement levels
        /// </summary>
        /// <returns>Returns the given players endorsements levels</returns>
        /// <response code="200">Returns players endorsements</response>
        /// <param name="player">User name of the person</param>
        /// <param name="platform">Selected game platform</param>
        [HttpGet("{platform}/{player}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Player<Endorsements>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ReponseTypes.NotFound))]
        public async Task<IActionResult> Endorsements(string player, Platform platform)
        {
            var data = await _overwatchProfileService.GetEndorsements(player, platform);
            if (data == null) return NotFound(new ReponseTypes.NotFound());

            return Ok(data);
        }
    }
}
