using BattleNet.Tools.Models;
using BattleNet.Tools.Services;
using Core.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BattleNet.Tools.API
{
    [GameToolRoute(typeof(ContentUIController), "[Controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Battle.net Content UI")]
    [Produces("application/json")]
    public class ContentUIController : ControllerBase
    {
        private readonly IContentUIService _contentUIService;

        public ContentUIController(IContentUIService contentUIService)
        {
            _contentUIService = contentUIService;
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
            var data = await _contentUIService.GetNextData();
            var stringJson = JsonConvert.SerializeObject(data.props.initialState.products.products);
            var exp = JsonConvert.DeserializeObject<List<ContentUINextModel.Root>>(stringJson);

            return Ok(exp);
        }
    }
}
