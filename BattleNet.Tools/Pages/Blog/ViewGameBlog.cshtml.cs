using System.Threading.Tasks;
using Core.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BattleNet.Tools.Pages.Blog
{
    public class ViewGameBlog : PageModel
    {
        private readonly DBContext _dbContext;

        public ViewGameBlog(DBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task OnGetAsync()
        {
            
        }
    }
}