using System.Threading.Tasks;
using BattleNet.Tools.Services;
using Core.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BattleNet.Tools.API
{
    [GameToolRoute(typeof(ContentBlogController))]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Battle.net Information")]
    [Produces("application/json")]
    public class ContentBlogController : ControllerBase
    {
        private readonly IContentBlogSerivce _contentBlogService;

        public ContentBlogController(IContentBlogSerivce contentBlogService)
        {
            _contentBlogService = contentBlogService;
        }

        /// <summary>
        ///     Gets games blog feed from Battle.net
        /// </summary>
        /// <returns>
        ///     List current blog post on Battle.net  for given game
        /// </returns>
        [HttpGet("{cxpProduct}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GameBlog(string cxpProduct, [FromQuery(Name = "page_size")] int pageSize = 10)
        {
            var data = await _contentBlogService.GetBlog(cxpProduct, pageSize);
            return Ok(data);
        }
    }
}