using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using BattleNet.Tools.Services;
using Core.Attributes;
using Core.Models;
using Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BattleNet.Tools.API
{
    [GameToolRoute(typeof(ContentBlogController), "blog/{slug}")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Battle.net Information")]
    [Produces("application/json")]
    public class ContentBlogController : ControllerBase
    {
        private readonly IContentBlogSerivce _contentBlogService;
        private readonly IGameParents _gameParents;

        public ContentBlogController(IContentBlogSerivce contentBlogService, IGameParents gameParents)
        {
            _contentBlogService = contentBlogService;
            _gameParents = gameParents;
        }

        /// <summary>
        ///     Blog Feeds
        /// </summary>
        /// <returns>
        ///     List current blog post on Battle.net  for given game
        /// </returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result<List<BlogPostItem>>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ReponseTypes.NotFound))]
        public async Task<IActionResult> GameBlog(string slug, [FromQuery(Name = "page_size")] int pageSize = 10)
        {
            var results = new Result<List<BlogPostItem>>
            {
                Data = new List<BlogPostItem>()
            };
            
            var game = await _gameParents.Get(slug);
            if (string.IsNullOrEmpty(game?.CxpProductId)) return NotFound(new ReponseTypes.NotFound());

            results.Name = game.Name;
            results.Slug = game.Slug;
            results.Logos = game.Logos;
            
            var data = await _contentBlogService.GetBlog(game.CxpProductId, pageSize);

            results.Data = data.Select(x => new BlogPostItem
            {
                Title = x.Properties.Title,
                Author = x.Properties.Author,
                Summary = x.Properties.Summary,
                PostId = x.ContentId,
                Category = x.Properties.Category,
                Updated = x.Properties.LastUpdated,
                Assets = new Assets
                {
                    ImageUrl = x.Properties.StaticAsset.ImageUrl,
                    ImageFileType = x.Properties.StaticAsset.ImageFileType,
                    ImageAltText = x.Properties.StaticAsset.ImageAltText
                },
                View = Url.Action("GameBlogPost", "ContentBlog",
                    new { slug = game.Slug, postId = x.ContentId },
                    Scheme()
                )
            }).ToList();
            
            var relations = new Dictionary<RelationTypes, string>();
            if(game.PatchNoteAreas is {Count: > 0})
            {
                relations[RelationTypes.PatchNotes] = Url.Action("List", "PatchNotes",
                    new { game = game.Slug },
                    Scheme()
                );
            }
            
            if(relations.Count > 0)
            {
                results.Relations = relations;
            }
            
            return Ok(results);
        }
        
        /// <summary>
        ///     Read Blog Post
        /// </summary>
        /// <returns>
        ///     Read the given blog post
        /// </returns>
        [HttpGet("{postId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result<BlogPostView>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ReponseTypes.NotFound))]
        public async Task<IActionResult> GameBlogPost(string slug, string postId)
        {
            var results = new Result<BlogPostView>
            {
                Data = new BlogPostView()
            };
            
            var game = await _gameParents.Get(slug);
            if (string.IsNullOrEmpty(game?.CxpProductId)) return NotFound(new ReponseTypes.NotFound());
            
            results.Name = game.Name;
            results.Slug = game.Slug;
            results.Logos = game.Logos;
            
            var relations = new Dictionary<RelationTypes, string>();
            if(game.PatchNoteAreas is {Count: > 0})
            {
                relations[RelationTypes.PatchNotes] = Url.Action("List", "PatchNotes",
                    new { game = game.Slug },
                    Scheme()
                );
            }
            relations[RelationTypes.BlogFeed] =  Url.Action("GameBlog", "ContentBlog",
                new { slug = game.Slug },
                Scheme()
            );
            
            if(relations.Count > 0)
            {
                results.Relations = relations;
            }


            var data = await _contentBlogService.GetPost(postId);
            if (string.IsNullOrEmpty(data?.Properties?.Content)) return NotFound(new ReponseTypes.NotFound());

            results.Data.Assets = new Assets
            {
                ImageUrl = data.Properties.StaticAsset.ImageUrl,
                ImageFileType = data.Properties.StaticAsset.ImageFileType,
                ImageAltText = data.Properties.StaticAsset.ImageAltText
            };

            results.Data.Title = data.Properties.Title;
            results.Data.Author = data.Properties.Author;
            results.Data.Summary = data.Properties.Summary;
            results.Data.PostId = data.ContentId;
            results.Data.Category = data.Properties.Category;
            results.Data.Updated = data.Properties.LastUpdated;
            results.Data.Content = data.Properties.Content;
            
            return Ok(results);
        }
        
        private string Scheme()
        {
            return HttpContext.Request.Host.Host.Contains("blizztrack", StringComparison.OrdinalIgnoreCase) ? "https" : "http";
        }

    }
    
    [DataContract]
    [JsonConverter(typeof(StringEnumConverter))]
    internal enum RelationTypes
    {
        PatchNotes,
        BlogFeed
    }
    
    internal class Result<T>
    {
        public string Name { get; set; }
        
        public string Slug { get; set; }
        
        public List<Icons> Logos { get; set; }
        
        /// <summary>
        ///     Relations to other pages
        /// </summary>
        public Dictionary<RelationTypes, string> Relations { get; set; } = null;
        
        public T Data { get; set; }
    }

    internal class BlogPostView
    {
        public Assets Assets { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Summary { get; set; }
        public string Category { get; set; }
        public DateTime Updated { get; set; }
        public string Content { get; set; }
        public string PostId { get; set; }
    }

    internal class BlogPostItem
    {
        public Assets Assets { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Summary { get; set; }
        public string PostId { get; set; }
        public string Category { get; set; }
        public DateTime Updated { get; set; }
        public string View { get; set; }
    }

    internal class Assets
    {
        public string ImageUrl { get; set; }
        public string ImageFileType { get; set; }
        public string ImageAltText { get; set; }
    }
}