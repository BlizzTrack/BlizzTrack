using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BattleNet.Tools.Services
{
    public interface IBlizzardAuthService
    {
        Task<string> GetAuthToken();
    }

    public class BlizzardAuthService : IBlizzardAuthService

    {
        private IBlizzardAuthService _blizzardAuthServiceImplementation;
        public async Task<string> GetAuthToken()
        {
            using var wc = new HttpClient();
            using var wcRes = await wc.GetAsync("https://content-ui.battle.net/en-us/browse/fenris");
            var rsltContent = await wcRes.Content.ReadAsStringAsync();
            return rsltContent.Split("window.blizzard = {accessToken:").Last().Split("}").First().Trim().Replace("'", "");
        }
    }
}