using Newtonsoft.Json;
using Overwatch.Tools.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Overwatch.Tools.Services
{
    public interface IOverwatchSearchService
    {
        Task<List<PlayerSearchResult<string>>> Search(string player);
    }

    public class OverwatchSearchService : IOverwatchSearchService
    {
        public async Task<List<PlayerSearchResult<string>>> Search(string player)
        {
            using var wc = new WebClient();

            var data = await wc.DownloadStringTaskAsync($"https://playoverwatch.com/en-us/search/account-by-name/{player}/");

            return JsonConvert.DeserializeObject<List<PlayerSearchResult<string>>>(data);
        }
    }
}
