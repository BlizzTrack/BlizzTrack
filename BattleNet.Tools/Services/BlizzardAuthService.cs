using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BattleNet.Tools.Models;

namespace BattleNet.Tools.Services
{
    public interface IBlizzardAuthService
    {
        Task<BattleNetConfigModel> GetAuthConfig();
    }

    public class BlizzardAuthService : IBlizzardAuthService
    {
        public async Task<BattleNetConfigModel> GetAuthConfig()
        {
            using var wc = new HttpClient();
            using var wcRes = await wc.GetAsync("https://content-ui.battle.net/en-us/browse/fenris");
            var rsltContent = await wcRes.Content.ReadAsStringAsync();
            
            var bearer = rsltContent.Split("window.blizzard = {accessToken:").Last().Split("}").First().Trim().Replace("'", "");
            var layoutCtsId = rsltContent.Split("\"CTS_DEFAULT_LAYOUT_TEMPLATE_ID\":\"").Last().Split("\"").First().Trim();
            var ctsUrl = rsltContent.Split("\"CTS_URL\":\"").Last().Split("\"").First().Trim();
            var ctsVersion = rsltContent.Split("\"CTS_VERSION\":\"").Last().Split("\"").First().Trim();
            
            return new BattleNetConfigModel()
            {
                Bearer = bearer,
                LayoutCtsId = layoutCtsId,
                AssetHost = ctsUrl,
                AssetHostVersion = ctsVersion
            };
        }
    
        public async Task<string> GetAuthToken()
        {
            using var wc = new HttpClient();
            using var wcRes = await wc.GetAsync("https://content-ui.battle.net/en-us/browse/fenris");
            var rsltContent = await wcRes.Content.ReadAsStringAsync();
            return rsltContent.Split("window.blizzard = {accessToken:").Last().Split("}").First().Trim().Replace("'", "");
        }
    }
}