using BattleNet.Tools.Models;
using BattleNet.Tools.Services;
using Core.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BattleNet.Tools.API
{
    [GameToolRoute(typeof(ContentUiController), "[Controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Battle.net Content UI")]
    [Produces("application/json")]
    public class ContentUiController : ControllerBase
    {
        private readonly IContentUiService _contentUiService;

        public ContentUiController(IContentUiService contentUiService)
        {
            _contentUiService = contentUiService;
        }

        /// <summary>
        ///     All games on Battle.net
        /// </summary>
        /// <returns>
        ///     A list list of all games on the new Battle.net launcher including hidden/disabled games
        /// </returns>
        [HttpGet("list")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ContentUINextModel.Root>))]
        public async Task<IActionResult> ListGames()
        {
            var data = await _contentUiService.GetNextData();
            var stringJson = JsonConvert.SerializeObject(data.products);
            var exp = JsonConvert.DeserializeObject<List<ContentUINextModel.Root>>(stringJson);

            return Ok(exp);
        }
    }
}
