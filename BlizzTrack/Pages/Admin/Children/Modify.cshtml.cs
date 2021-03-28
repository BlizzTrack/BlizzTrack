using Core.Extensions;
using Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Core.Models;
using GameParents = Core.Models.GameParents;

namespace BlizzTrack.Pages.Admin.Children
{
    public class GameInfoModel
    {
        [Required]
        [Display(Name = "Game Name")]
        public string GameName { get; set; }

        [Display(Name = "Game Slug")]
        public string GameSlug { get; set; }
        
        [Display(Name =  "Code of Game Parent")]
        [Required]
        public string ParentCode { get; set; }
    }

    public class ModifyModel : PageModel
    {
        private readonly IGameChildren _gameChildren;
        private readonly IGameParents _gameParents;
        private readonly DBContext _dbContext;

        public ModifyModel(IGameChildren gameChildren, IGameParents gameParents, DBContext dbContext)
        {
            _gameChildren = gameChildren;
            _gameParents = gameParents;
            _dbContext = dbContext;
        }

        [BindProperty]
        public GameInfoModel GameInfoModel { get; set; }

        [BindProperty(SupportsGet = true, Name = "code")]
        public string ParentCode { get; set; }

        public Core.Models.GameChildren GameInfo;

        public List<GameParents> GameParents { get; set; }
        
        public async Task OnGetEditAsync()
        {
            GameInfo = await _gameChildren.Get(ParentCode); //_gameParents.Get(ParentCode);
            if (GameInfo == null)
            {
                NotFound();
                return;
            }

            GameParents = await _dbContext.GameParents.AsNoTracking().ToListAsync();

            ViewData["Title"] = $"Editing {GameInfo.Name}";

            GameInfoModel ??= new GameInfoModel()
            {
                GameName = GameInfo.Name,
                GameSlug = GameInfo.Slug ?? GameInfo.Name.Slugify(),
                ParentCode = GameInfo.ParentCode
            };
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            GameInfo = await _gameChildren.Get(ParentCode); //_gameParents.Get(ParentCode);
            GameParents = await _dbContext.GameParents.AsNoTracking().ToListAsync();

            if (GameInfo == null)
            {
                return NotFound();
            }

            var company = GameParents.FirstOrDefault(x => x.Code == GameInfoModel.ParentCode);
            if (company == null)
            {
                return Page();
            }
            
            GameInfo.Name = GameInfoModel.GameName;
            GameInfo.Slug = string.IsNullOrEmpty(GameInfoModel.GameSlug) ? GameInfo.Name.Slugify() : GameInfoModel.GameSlug;
            GameInfo.ParentCode = company.Code;

            _dbContext.GameChildren.Update(GameInfo);
            await _dbContext.SaveChangesAsync();

            ViewData["Title"] = $"Editing {GameInfo.Name}";

            return Redirect($"/admin/game-children/modify?handler=Edit&code={GameInfo.Code}");
        }
    }
}