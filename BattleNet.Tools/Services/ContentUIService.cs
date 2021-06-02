using System.Linq;
using AngleSharp.Html.Parser;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace BattleNet.Tools.Services
{
    public interface IContentUiService
    {
        Task<dynamic> GetNextData();
    }

    public class ContentUiService : IContentUiService
    {
        private readonly IBlizzardAuthService _authService;

        public ContentUiService(IBlizzardAuthService authService)
        {
            _authService = authService;
        }
        
        public async Task<dynamic> GetNextData()
        {
            var config = await _authService.GetAuthConfig();

            using var wc = new HttpClient();
            wc.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.Bearer);
            using var client = await wc.GetAsync("https://us.api.blizzard.com/content-api/v1/cxpProducts");
            var data = await client.Content.ReadAsStringAsync();
            
            return Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(data);
        }
    }
}
