using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using BattleNet.Tools.Models;
using Core.Models;
using Newtonsoft.Json;

namespace BattleNet.Tools.Services
{
    public interface IContentBlogSerivce
    {
        Task<IEnumerable<ContentBlogModel.ContentItem>> GetBlog(string cxpProduct, int pageSize);
        Task<ContentBlogItemModel> GetPost(string postId);
        Task<IEnumerable<ContentBlogModel.ContentItem>> GetRelatedPost(string postId);
    }
    
    public class ContentBlogService : IContentBlogSerivce
    {
        private readonly IBlizzardAuthService _authService;

        public ContentBlogService(IBlizzardAuthService authService)
        {
            _authService = authService;
        }

        public async Task<IEnumerable<ContentBlogModel.ContentItem>> GetBlog(string cxpProduct, int pageSize)
        {
            var config = await _authService.GetAuthConfig();
            
            using var wc = new HttpClient();
            wc.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.Bearer);
            using var client = await wc.GetAsync($"{config.AssetHost}{config.AssetHostVersion}/layoutTemplates/{config.LayoutCtsId}/layout?limit={pageSize}&offset=0&includeFeed=true&feedLimit={pageSize}&cxpProductId={cxpProduct}");
            var data = await client.Content.ReadAsStringAsync();
            var f = JsonConvert.DeserializeObject<ContentBlogModel.Root>(data);

            return f?.Feed.ContentItems;
        }
        
        public async Task<ContentBlogItemModel> GetPost(string postId)
        {
            var config = await _authService.GetAuthConfig();
            
            using var wc = new HttpClient();
            wc.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.Bearer);
            using var client = await wc.GetAsync($"{config.AssetHost}{config.AssetHostVersion}/content/blogs/{postId}");
            var data = await client.Content.ReadAsStringAsync();
            var f = JsonConvert.DeserializeObject<ContentBlogItemModel>(data);

            return f;
        }

        public async Task<IEnumerable<ContentBlogModel.ContentItem>> GetRelatedPost(string postId)
        {
            var config = await _authService.GetAuthConfig();
            
            using var wc = new HttpClient();
            wc.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.Bearer);
            using var client = await wc.GetAsync($"{config.AssetHost}{config.AssetHostVersion}/content/blogs/{postId}/relatedContent?contentId={postId}");
            var data = await client.Content.ReadAsStringAsync();
            var f = JsonConvert.DeserializeObject<ContentBlogModel.Root>(data);

            return f?.ContentItems;
        }
    }
}