using Core.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Overwatch.Tools.API
{
    [GameToolRoute(typeof(EndorsmentController))]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Overwatch Game Data")]
    [Produces("application/json")]
    public class EndorsmentController : ControllerBase
    {
        [HttpGet("[action]")]
        public IActionResult Endorsements()
        {
            return Ok("ok");
        }
    }
}
