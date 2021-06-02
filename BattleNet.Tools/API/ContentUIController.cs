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
    [GameToolRoute(typeof(ContentUiController), "ui")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Battle.net Information")]
    [Produces("application/json")]
    public class ContentUiController : ControllerBase
    {
        private readonly IContentUiService _contentUiService;

        public ContentUiController(IContentUiService contentUiService)
        {
            _contentUiService = contentUiService;
        }

        /// <summary>
        ///    List Games
        /// </summary>
        /// <returns>
        ///     A list of all games on the new Battle.net launcher including hidden/disabled games
        /// </returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ContentUINextModel.Root>))]
        public async Task<IActionResult> ListGames()
        {
            var data = await _contentUiService.GetNextData();
            //var stringJson = JsonConvert.SerializeObject(data.products);
           // var exp = JsonConvert.DeserializeObject<List<ContentUINextModel.Root>>(stringJson);

            return Ok(data.products);
        }
    }
}
