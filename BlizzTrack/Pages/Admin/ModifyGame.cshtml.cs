using BlizzTrack.Helpers;
using Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureManagement.Mvc;
using Minio;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BlizzTrack.Pages.Admin
{
    public class GameInfoModel
    {
        [Required]
        [Display(Name = "Game Name")]
        public string GameName { get; set; }

        [Display(Name = "Game Website")]
        [Url]
        public string GameWebsite { get; set; }

        [Display(Name = "Service Alert Icon Path")]
        public string ServiceAlertPath { get; set; }
    }

    [FeatureGate(nameof(FeatureFlags.AdminPanel))]
    [Authorize(Roles = "Admin")]
    public class ModifyGameModel : PageModel
    {
        private readonly IGameConfig _gameConfig;
        private readonly MinioClient _minioClient;
        private readonly string _bucket;
        private readonly Core.Models.DBContext _dbContext;

        public ModifyGameModel(IGameConfig gameConfig, MinioClient minioClient, IConfiguration config, Core.Models.DBContext dbContext)
        {
            _gameConfig = gameConfig;
            _minioClient = minioClient;
            _bucket = config.GetValue("AWS:BucketName", "");
            _dbContext = dbContext;
        }

        [BindProperty(SupportsGet = true, Name = "game")]
        public string GameCode { get; set; }

        [BindProperty]
        public GameInfoModel GameInfoModel { get; set; }

        public Core.Models.GameConfig GameInfo;

        public async Task OnGetAsync()
        {
            GameInfo = await _gameConfig.Get(GameCode);
            if (GameInfo == null)
            {
                NotFound();
                return;
            }

            GameInfoModel ??= new GameInfoModel
            {
                GameName = GameInfo.GetName(),
                GameWebsite = GameInfo.Website,
                ServiceAlertPath = GameInfo.ServiceURL
            };
        }

        public async Task<IActionResult> OnPostUpdateGameAsync()
        {
            GameInfo = await _gameConfig.Get(GameCode);

            if (!ModelState.IsValid)
            {
                return Page();
            }

            GameInfo.Name = GameInfoModel.GameName;
            GameInfo.Website = GameInfoModel.GameWebsite;
            GameInfo.ServiceURL = GameInfoModel.ServiceAlertPath;

            _dbContext.GameConfigs.Update(GameInfo);
            await _dbContext.SaveChangesAsync();

            return Redirect(HttpContext.Request.Path);
        }
    }
}