using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Minio;

namespace BlizzTrack.Pages.Admin.Parents
{
    public class GameInfoModel
    {
        [Required]
        [Display(Name = "Game Name")]
        public string GameName { get; set; }

        [Required]
        [Display(Name = "Game Starts With Code (ex: pro)")]
        public string GameCode { get; set; }

        [Display(Name = "Game Website")]
        [Url]
        public string GameWebsite { get; set; }

        [Display(Name = "Game Child Overirde")]
        public string GameChildOverride { get; set; }

        [Display(Name = "Game Asset (Images)")]
        public IFormFile Icon { get; set; }

        public Core.Models.GameParents ToGameParents => new Core.Models.GameParents()
        {
            Name = GameName,
            Code = GameCode?.ToLower(),
        };
    }

    public class ModifyModel : PageModel
    {
        private readonly IGameParents _gameParents;
        private readonly MinioClient _minioClient;
        private readonly string _bucket;
        private readonly Core.Models.DBContext _dbContext;
        public ModifyModel(IGameParents gameParents, MinioClient minioClient, IConfiguration config, Core.Models.DBContext dbContext)
        {
            _gameParents = gameParents;
            _minioClient = minioClient;
            _bucket = config.GetValue("AWS:BucketName", "");
            _dbContext = dbContext;
        }

        [BindProperty]
        public GameInfoModel GameInfoModel { get; set; }

        [BindProperty(SupportsGet = true, Name = "code")]
        public string ParentCode { get; set; }

        public Core.Models.GameParents GameInfo;


        public async Task OnGetNewAsync()
        {
            ViewData["Title"] = "Add New Parent";

            if (GameInfoModel == null) GameInfoModel = new GameInfoModel();
            GameInfo = new Core.Models.GameParents
            {
                Logos = new System.Collections.Generic.List<Core.Models.Icons>()
            };
        }

        public async Task<IActionResult> OnPostNewAsync()
        {
            ViewData["Title"] = "Add New Parent";

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var exist = await _gameParents.Get(GameInfoModel.GameCode);
            if(exist?.Code == GameInfoModel.GameCode.ToLower())
            {
                return Page();
            }

            var parent = GameInfoModel.ToGameParents;
            parent.Logos = new List<Core.Models.Icons>();

            if(GameInfoModel.Icon != null)
            {
                if (!GameInfoModel.Icon.ContentType.StartsWith("image/"))
                {
                    ModelState.AddModelError("GameInfoModel.Icon", "Must be an image");
                    return Page();
                }

                var dest = Path.Join("bt", "logos", "games", $"{Guid.NewGuid()}{Path.GetExtension(GameInfoModel.Icon.FileName)}").Replace("\\", "/").TrimStart('/');

                using var ms = GameInfoModel.Icon.OpenReadStream();

                await _minioClient.PutObjectAsync(_bucket, dest, ms, ms.Length, GameInfoModel.Icon.ContentType, new Dictionary<string, string> { { "x-amz-acl", "public-read" } });

                parent.Logos.Add(new Core.Models.Icons()
                {
                    Type = GameInfoModel.Icon.ContentType,
                    URL = $"https://cdn.blizztrack.com/{dest}",
                    OriginalName = GameInfoModel.Icon.FileName
                });
            }

            parent.ChildrenOverride = new List<string>();

            if(!string.IsNullOrEmpty(GameInfoModel.GameChildOverride))
                foreach (var item in GameInfoModel.GameChildOverride.Split(','))
                {
                    if (string.IsNullOrWhiteSpace(item.Trim()))
                        continue;
                    parent.ChildrenOverride.Add(item.Trim().ToLower());
                }


            await _gameParents.Add(parent);

            return Redirect($"/admin/game-parents/modify?handler=Edit&code={parent.Code}");
        }

        public async Task OnGetEditAsync()
        {
            GameInfo = await _gameParents.Get(ParentCode);
            if (GameInfo == null)
            {
                NotFound();
                return;
            }

            ViewData["Title"] = $"Editing {GameInfo.Name}";

            if (GameInfoModel == null)
                GameInfoModel = new GameInfoModel()
                {
                    GameName = GameInfo.Name,
                    GameCode = GameInfo.Code,
                    GameChildOverride = string.Join(", ", GameInfo.ChildrenOverride ?? new List<string>())
                };
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            GameInfo = await _gameParents.Get(ParentCode);
            if (GameInfo == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (GameInfoModel.Icon != null)
            {
                if (!GameInfoModel.Icon.ContentType.StartsWith("image/"))
                {
                    ModelState.AddModelError("GameInfoModel.Icon", "Must be an image");
                    return Page();
                }

                var dest = Path.Join("bt", "logos", "games", $"{Guid.NewGuid()}{Path.GetExtension(GameInfoModel.Icon.FileName)}").Replace("\\", "/").TrimStart('/');

                using var ms = GameInfoModel.Icon.OpenReadStream();

                await _minioClient.PutObjectAsync(_bucket, dest, ms, ms.Length, GameInfoModel.Icon.ContentType, new Dictionary<string, string> { { "x-amz-acl", "public-read" } });

                if (GameInfo.Logos == null) GameInfo.Logos = new List<Core.Models.Icons>();

                var exist = GameInfo.Logos.FirstOrDefault(x => x.Type == GameInfoModel.Icon.ContentType);
                if (exist == null)
                {
                    GameInfo.Logos.Add(new Core.Models.Icons()
                    {
                        Type = GameInfoModel.Icon.ContentType,
                        URL = $"https://cdn.blizztrack.com/{dest}",
                        OriginalName = GameInfoModel.Icon.FileName
                    });
                }
                else
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
            GameInfo.Code = GameInfoModel.GameCode;
            GameInfo.ChildrenOverride = new List<string>();

            if(!string.IsNullOrWhiteSpace(GameInfoModel.GameChildOverride))
                foreach(var item in GameInfoModel.GameChildOverride.Split(','))
                {
                    if (string.IsNullOrWhiteSpace(item.Trim()))
                        continue;

                    GameInfo.ChildrenOverride.Add(item.Trim().ToLower());
                }

            _dbContext.GameParents.Update(GameInfo);
            await _dbContext.SaveChangesAsync();

            ViewData["Title"] = $"Editing {GameInfo.Name}";

            return Redirect($"/admin/game-parents/modify?handler=Edit&code={GameInfo.Code}");
        }

        public async Task<IActionResult> OnPostDeleteAssetAsync(string asset_url, string asset_type)
        {
            GameInfo = await _gameParents.Get(ParentCode);
            if (GameInfo.Logos != null)
            {
                var asset = GameInfo.Logos.FirstOrDefault(x => x.URL == asset_url && x.Type == asset_type);
                GameInfo.Logos.Remove(asset);

                var path = new Uri(asset.URL);
                var filePath = path.AbsolutePath.TrimStart('/');
                await _minioClient.RemoveObjectAsync(_bucket, filePath);

                await _gameParents.Update(GameInfo);
            }

            return Redirect($"/admin/game-parents/modify?handler=Edit&code={GameInfo.Code}");
        }
    }
}