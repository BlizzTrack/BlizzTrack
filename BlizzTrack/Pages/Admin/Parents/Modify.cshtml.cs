using Core.Extensions;
using Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Minio;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Core.Models;

namespace BlizzTrack.Pages.Admin.Parents
{
    public class GameInfoModel
    {
        [Required]
        [Display(Name = "Game Name")]
        public string GameName { get; set; }

        [Display(Name = "Game Slug")]
        public string GameSlug { get; set; }
        
        [Display(Name =  "Company of Game")]
        [Required]
        public int GameCompany { get; set; }

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

        [Display(Name = "Patch Note Code")]
        public string PatchNoteCode { get; set; }

        [Display(Name = "Patch Note Tool (don't touch without looking at workers tool set)")]
        public string PatchNoteTool { get; set; } = "legacy";

        [Display(Name = "Patch Note Types")]
        public string PatchNoteTypes { get; set; }

        [Display(Name = "Catalog Manifest ID")]
        public string CatalogManifestID { get; set; }

        [Display(Name = "Visible")]
        public bool Visible { get; set; }
        
        [Display(Name =  "About this game")]
        public string GameAbout { get; set; }

        public Core.Models.GameParents ToGameParents => new()
        {
            Name = GameName,
            Code = GameCode?.ToLower(),
            PatchNoteTool = PatchNoteTool,
            Website = GameWebsite,
            PatchNoteCode = PatchNoteCode,
            Visible = Visible,
        };
    }

    public class ModifyModel : PageModel
    {
        private readonly IGameParents _gameParents;
        private readonly MinioClient _minioClient;
        private readonly string _bucket;
        private readonly DBContext _dbContext;
        private readonly IGameCompanies _gameCompanies;

        public ModifyModel(IGameParents gameParents, MinioClient minioClient, IConfiguration config, DBContext dbContext, IGameCompanies gameCompanies)
        {
            _gameParents = gameParents;
            _minioClient = minioClient;
            _bucket = config.GetValue("AWS:BucketName", "");
            _dbContext = dbContext;
            _gameCompanies = gameCompanies;
        }

        [BindProperty]
        public GameInfoModel GameInfoModel { get; set; }

        [BindProperty(SupportsGet = true, Name = "code")]
        public string ParentCode { get; set; }

        public Core.Models.GameParents GameInfo;

        public List<string> ManifestIDs { get; set; }
        
        public List<GameCompany> GameCompanies { get; set; }

        public async Task OnGetNewAsync()
        {
            ViewData["Title"] = "Add New Parent";

            if (GameInfoModel == null) GameInfoModel = new GameInfoModel();
            GameInfo = new Core.Models.GameParents
            {
                Logos = new List<Icons>(),
                Visible = true
            };

            ManifestIDs = await _dbContext.Catalogs.Where(x => x.Type == Core.Models.CatalogType.Fragment).GroupBy(x => x.Name).Select(x => x.Key).ToListAsync();
            GameCompanies = await _dbContext.GameCompanies.AsNoTracking().ToListAsync();
        }

        public async Task<IActionResult> OnPostNewAsync()
        {
            ViewData["Title"] = "Add New Parent";

            ManifestIDs = await _dbContext.Catalogs.Where(x => x.Type == CatalogType.Fragment).GroupBy(x => x.Name).Select(x => x.Key).ToListAsync();
            GameCompanies = await _dbContext.GameCompanies.AsNoTracking().ToListAsync();
            
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var exist = await _gameParents.Get(GameInfoModel.GameCode);
            if (exist?.Code == GameInfoModel.GameCode.ToLower())
            {
                return Page();
            }

            var parent = GameInfoModel.ToGameParents;
            parent.ManifestID = GameInfoModel.CatalogManifestID;

            parent.Logos = new List<Icons>();

            if (GameInfoModel.Icon != null)
            {
                if (!GameInfoModel.Icon.ContentType.StartsWith("image/"))
                {
                    ModelState.AddModelError("GameInfoModel.Icon", "Must be an image");
                    return Page();
                }

                var dest = Path.Join("bt", "logos", "games", $"{Guid.NewGuid()}{Path.GetExtension(GameInfoModel.Icon.FileName)}").Replace("\\", "/").TrimStart('/');

                await using var ms = GameInfoModel.Icon.OpenReadStream();

                await _minioClient.PutObjectAsync(_bucket, dest, ms, ms.Length, GameInfoModel.Icon.ContentType, new Dictionary<string, string> { { "x-amz-acl", "public-read" } });

                parent.Logos.Add(new Icons()
                {
                    Type = GameInfoModel.Icon.ContentType,
                    URL = $"https://cdn.blizztrack.com/{dest}",
                    OriginalName = GameInfoModel.Icon.FileName
                });
            }

            parent.ChildrenOverride = new List<string>();
            parent.PatchNoteAreas = new List<string>();

            if (!string.IsNullOrEmpty(GameInfoModel.GameChildOverride))
                foreach (var item in GameInfoModel.GameChildOverride.Split(','))
                {
                    if (string.IsNullOrWhiteSpace(item.Trim()))
                        continue;
                    parent.ChildrenOverride.Add(item.Trim().ToLower());
                }

            if (!string.IsNullOrEmpty(GameInfoModel.PatchNoteTypes))
                foreach (var item in GameInfoModel.PatchNoteTypes.Split(','))
                {
                    if (string.IsNullOrWhiteSpace(item.Trim()))
                        continue;
                    parent.PatchNoteAreas.Add(item.Trim().ToLower());
                }

            if (string.IsNullOrEmpty(GameInfoModel.GameSlug))
            {
                parent.Slug = GameInfoModel.GameName.Slugify();
            }
            else
            {
                parent.Slug = GameInfoModel.GameSlug;
            }

            parent.Visible = true;
            parent.About = GameInfoModel.GameAbout;
            
            await _gameParents.Add(parent);

            return Redirect($"/admin/game-parents/modify?handler=Edit&code={parent.Code}");
        }

        public async Task OnGetEditAsync()
        {
            GameInfo = await _dbContext.GameParents.AsNoTracking().Include(x => x.Owner).FirstOrDefaultAsync(x => x.Code == ParentCode); //_gameParents.Get(ParentCode);
            if (GameInfo == null)
            {
                NotFound();
                return;
            }

            ManifestIDs = await _dbContext.Catalogs.GroupBy(x => x.Name).Select(x => x.Key).ToListAsync();
            GameCompanies = await _dbContext.GameCompanies.AsNoTracking().ToListAsync();

            ViewData["Title"] = $"Editing {GameInfo.Name}";

            GameInfoModel ??= new GameInfoModel()
            {
                GameName = GameInfo.Name,
                GameSlug = GameInfo.Slug ?? GameInfo.Name.Slugify(),
                GameCode = GameInfo.Code,
                GameWebsite = GameInfo.Website,
                GameChildOverride = string.Join(", ", GameInfo.ChildrenOverride ?? new List<string>()),
                PatchNoteTypes = string.Join(", ", GameInfo.PatchNoteAreas ?? new List<string>()),
                PatchNoteTool = GameInfo.PatchNoteTool,
                PatchNoteCode = GameInfo.PatchNoteCode,
                CatalogManifestID = GameInfo.ManifestID ?? GameInfo.Code,
                GameAbout = GameInfo.About,
                GameCompany = GameInfo.Owner.Id,
                Visible = GameInfo.Visible ?? true,
            };
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            GameInfo = await _dbContext.GameParents.AsNoTracking().FirstOrDefaultAsync(x => x.Code == ParentCode); //_gameParents.Get(ParentCode);
            GameCompanies = await _dbContext.GameCompanies.AsNoTracking().ToListAsync();

            if (GameInfo == null)
            {
                return NotFound();
            }

            var company = GameCompanies.FirstOrDefault(x => x.Id == GameInfoModel.GameCompany);
            if (company == null)
            {
                return Page();
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

                await using var ms = GameInfoModel.Icon.OpenReadStream();

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

            GameInfo.PatchNoteTool = GameInfoModel.PatchNoteTool;
            GameInfo.Name = GameInfoModel.GameName;
            GameInfo.Code = GameInfoModel.GameCode;
            GameInfo.Website = GameInfoModel.GameWebsite;
            GameInfo.PatchNoteCode = GameInfoModel.PatchNoteCode;
            GameInfo.ChildrenOverride = new List<string>();
            GameInfo.PatchNoteAreas = new List<string>();
            GameInfo.ManifestID = GameInfoModel.CatalogManifestID;
            GameInfo.Visible = true;
            GameInfo.About = GameInfoModel.GameAbout;
            GameInfo.Owner = company;

            if (!string.IsNullOrWhiteSpace(GameInfoModel.GameChildOverride))
                foreach (var item in GameInfoModel.GameChildOverride.Split(','))
                {
                    if (string.IsNullOrWhiteSpace(item.Trim()))
                        continue;

                    GameInfo.ChildrenOverride.Add(item.Trim().ToLower());
                }

            if (!string.IsNullOrWhiteSpace(GameInfoModel.PatchNoteTypes))
                foreach (var item in GameInfoModel.PatchNoteTypes.Split(','))
                {
                    if (string.IsNullOrWhiteSpace(item.Trim()))
                        continue;

                    GameInfo.PatchNoteAreas.Add(item.Trim().ToLower());
                }

            if (string.IsNullOrEmpty(GameInfoModel.GameSlug))
            {
                GameInfo.Slug = GameInfo.Name.Slugify();
            } else
            {
                GameInfo.Slug = GameInfoModel.GameSlug;
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