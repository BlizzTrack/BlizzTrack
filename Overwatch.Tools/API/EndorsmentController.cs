using Core.Attributes;
using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Overwatch.Tools.Models;
using Overwatch.Tools.Services;
using System.Threading.Tasks;

namespace Overwatch.Tools.API
{
    [GameToolRoute(typeof(EndorsmentController))]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Overwatch Game Data")]
    [Produces("application/json")]
    public class EndorsmentController : ControllerBase
    {
        private readonly IOverwatchProfileService overwatchProfileService;

        public EndorsmentController(IOverwatchProfileService overwatchProfileService)
        {
            this.overwatchProfileService = overwatchProfileService;
        }


        /// <summary>
        ///     Get players endormesment levels
        /// </summary>
        /// <returns>Returns the given players endorsements levels</returns>
        /// <response code="200">Returns latest items for given seqn</response>
        /// <param name="player">User name of the person</param>
        /// <param name="platform">Selected game platform</param>
        [HttpGet("[action]/{platform}/{player}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Player<Endorsements>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ReponseTypes.NotFound))]
        public async Task<IActionResult> Endorsements(string player, Platform platform)
        {
            var data = await overwatchProfileService.GetEndorsments(player, platform);
            if (data == null) return NotFound(new ReponseTypes.NotFound());

            return Ok(data);
        }
    }
}
