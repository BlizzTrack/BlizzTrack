using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BattleNet.Tools.Models;
using BattleNet.Tools.Services;
using Core.Models;
using Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BattleNet.Tools.Pages.Blog
{
    public class ReadGameBlogPost : PageModel
    {
        private readonly IGameParents _gameParents;
        private readonly IContentBlogSerivce _contentBlog;
        private readonly DBContext _dbContext;

        public ReadGameBlogPost(IGameParents gameParents, IContentBlogSerivce contentBlog, DBContext dbContext)
        {
            _gameParents = gameParents;
            _contentBlog = contentBlog;
            _dbContext = dbContext;
        }

        [BindProperty(SupportsGet = true, Name = "slug")]
        public string Slug { get; set; }
        
        [BindProperty(SupportsGet = true, Name = "post")]
        public string PostId { get; set; }
        
        public Core.Models.GameParents Parent { get; private set; }
        
        public List<Core.Models.GameParents> Parents { get; set; }
        
        public List<ContentBlogModel.ContentItem> Related { get; private set; }
        
        public ContentBlogItemModel Post { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            Parent = await _gameParents.Get(Slug);
            if (string.IsNullOrEmpty(Parent?.CxpProductId)) 
            {
                return NotFound();
            }

            Post = await _contentBlog.GetPost(PostId);
            Related = (await _contentBlog.GetRelatedPost(PostId)).ToList();

            Parents = await _dbContext.GameParents
                .Where(x => Related.Select(z => z.Properties.CxpProduct.Segment).Contains(x.CxpProductId))
                .ToListAsync();
            return Page();
        }
    }
}