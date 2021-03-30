using AngleSharp.Html.Parser;
using System.Net.Http;
using System.Threading.Tasks;

namespace BattleNet.Tools.Services
{
    public interface IContentUiService
    {
        Task<dynamic> GetNextData();
    }

    public class ContentUiService : IContentUiService
    {
        private readonly HtmlParser _parser = new();

        public async Task<dynamic> GetNextData()
        {
            using var wc = new HttpClient();
            using var wcRes = await wc.GetAsync("https://content-ui.battle.net/en-us/browse/fenris");
            if (!wcRes.IsSuccessStatusCode) return null;
            var rsltContent = await wcRes.Content.ReadAsStringAsync();

            using var doc = await _parser.ParseDocumentAsync(rsltContent);

            var data = doc.QuerySelector("script#__NEXT_DATA__");

            return Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(data?.TextContent ?? string.Empty);
        }
    }
}
