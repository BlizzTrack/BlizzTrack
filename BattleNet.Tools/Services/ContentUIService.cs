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
        public async Task<dynamic> GetNextData()
        {
            using var wc = new HttpClient();
            using var wcRes = await wc.GetAsync("https://content-ui.battle.net/en-us/browse/fenris");
            var rsltContent = await wcRes.Content.ReadAsStringAsync();
            var accessToken = rsltContent.Split("window.blizzard = {accessToken:").Last().Split("}").First().Trim().Replace("'", "");

            wc.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            using var client = await wc.GetAsync("https://us.api.blizzard.com/content-api/v1/cxpProducts");
            var data = await client.Content.ReadAsStringAsync();
            
            return Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(data);
        }
    }
}
