using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BlizzTrack.Helpers;
using Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureManagement.Mvc;
using Minio;

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

        [Display(Name = "Game Asset (Images)")]
        public IFormFile Icon { get; set; }

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

            if(GameInfoModel == null)
                GameInfoModel = new GameInfoModel
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

            if(GameInfoModel.Icon != null)
            {
                if(!GameInfoModel.Icon.ContentType.StartsWith("image/"))
                {
                    ModelState.AddModelError("GameInfoModel.Icon", "Must be an image");
                    return Page();
                }

                var dest = Path.Join("bt", "logos", "games", $"{Guid.NewGuid()}{Path.GetExtension(GameInfoModel.Icon.FileName)}").Replace("\\", "/").TrimStart('/');

                using var ms = GameInfoModel.Icon.OpenReadStream();

                await _minioClient.PutObjectAsync(_bucket, dest, ms, ms.Length, GameInfoModel.Icon.ContentType, new Dictionary<string, string> { { "x-amz-acl", "public-read" } });

                if (GameInfo.Logos == null) GameInfo.Logos = new List<Core.Models.Icons>();

                var exist = GameInfo.Logos.FirstOrDefault(x => x.Type == GameInfoModel.Icon.ContentType);
                if(exist == null)
                {
                    GameInfo.Logos.Add(new Core.Models.Icons()
                    {
                        Type = GameInfoModel.Icon.ContentType,
                        URL = $"https://cdn.blizztrack.com/{dest}",
                        OriginalName = GameInfoModel.Icon.FileName
                    });
                } else
                {
                    // Delete old image then set URL to the new one
                    var path = new Uri(exist.URL);
                    var filePath = path.AbsolutePath.TrimStart('/');
                    await _minioClient.RemoveObjectAsync(_bucket, filePath);

                    exist.URL = $"https://cdn.blizztrack.com/{dest}";
                    exist.OriginalName = GameInfoModel.Icon.FileName;
                }
            }

            GameInfo.Name = GameInfoModel.GameName;
            GameInfo.Website = GameInfoModel.GameWebsite;
            GameInfo.ServiceURL = GameInfoModel.ServiceAlertPath;

            _dbContext.GameConfigs.Update(GameInfo);
            await _dbContext.SaveChangesAsync();

            return Redirect(HttpContext.Request.Path);
        }

        public async Task<IActionResult> OnPostDeleteAssetAsync(string asset_url, string asset_type) 
        {
            GameInfo = await _gameConfig.Get(GameCode);
            if(GameInfo.Logos != null)
            {
                var asset = GameInfo.Logos.FirstOrDefault(x => x.URL == asset_url && x.Type == asset_type);
                GameInfo.Logos.Remove(asset);

                var path = new Uri(asset.URL);
                var filePath = path.AbsolutePath.TrimStart('/');
                await _minioClient.RemoveObjectAsync(_bucket, filePath);

                await _gameConfig.Update(GameInfo);
            }

            if (GameInfoModel == null)
                GameInfoModel = new GameInfoModel
                {
                    GameName = GameInfo.GetName(),
                    GameWebsite = GameInfo.Website,
                    ServiceAlertPath = GameInfo.ServiceURL
                };

            return Redirect(HttpContext.Request.Path);
        }

    }
}
