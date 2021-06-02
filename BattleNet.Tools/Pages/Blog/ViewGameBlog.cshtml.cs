using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BattleNet.Tools.Models;
using BattleNet.Tools.Services;
using Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GameParents = Core.Models.GameParents;

namespace BattleNet.Tools.Pages.Blog
{
    public class ViewGameBlog : PageModel
    {
        private readonly IGameParents _gameParents;
        private readonly IContentBlogSerivce _contentBlog;

        public ViewGameBlog(IGameParents gameParents, IContentBlogSerivce contentBlog)
        {
            _gameParents = gameParents;
            _contentBlog = contentBlog;
        }

        [BindProperty(SupportsGet = true, Name = "slug")]
        public string Slug { get; set; }
        
        public GameParents Parent { get; private set; }
        
        public List<ContentBlogModel.ContentItem> Items { get; private set; }

        public async Task<IActionResult> OnGetAsync()
        {
            Parent = await _gameParents.Get(Slug);
            if (string.IsNullOrEmpty(Parent?.CxpProductId))
            {
                return NotFound();
            }

            Items = (await _contentBlog.GetBlog(Parent.CxpProductId, 10)).ToList();
            return Page();
        }
    }
}