using Core.Attributes;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Overwatch.Tools.API
{
    [DataContract]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Plateform
    {
        PC,
        PSN,
        XBL
    }

    [GameToolRoute(typeof(EndorsmentController))]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Overwatch Game Data")]
    [Produces("application/json")]
    public class EndorsmentController : ControllerBase
    {
        /// <summary>
        ///     Get players endormesment levels
        /// </summary>
        /// <returns>Returns the given players endorsements levels</returns>
        /// <response code="200">Returns latest items for given seqn</response>
        /// <param name="player">User name of the person</param>
        /// <param name="plateform">Selected game platform</param>
        [HttpGet("[action]")]
        public IActionResult Endorsements(string player, Plateform plateform)
        {
            return Ok("ok");
        }
    }
}
