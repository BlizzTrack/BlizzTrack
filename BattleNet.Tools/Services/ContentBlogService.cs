using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using BattleNet.Tools.Models;
using Newtonsoft.Json;

namespace BattleNet.Tools.Services
{
    public interface IContentBlogSerivce
    {
        Task<ContentBlogModel.Root> GetBlog(string cxpProduct, int pageSize);
    }
    
    public class ContentBlogService : IContentBlogSerivce
    {
        private readonly IBlizzardAuthService _authService;

        public ContentBlogService(IBlizzardAuthService authService)
        {
            _authService = authService;
        }

        public async Task<ContentBlogModel.Root> GetBlog(string cxpProduct, int pageSize)
        {
            using var wc = new HttpClient();
            wc.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _authService.GetAuthToken());
            using var client = await wc.GetAsync($"https://us.api.blizzard.com/content-api/v1/layoutTemplates/blt387258f1fd825b72/layout?limit=10&offset=0&includeFeed=true&feedLimit={pageSize}&cxpProductId={cxpProduct}");
            var data = await client.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<ContentBlogModel.Root>(data);
        }
    }
}