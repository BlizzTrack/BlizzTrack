using StackExchange.Redis.Extensions.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace BlizzTrack.Services
{
    public interface IBlizzardAlerts
    {
        Task<string> Get(string code);

        Task<string> ExperimentalOnline();
    }

    public class BlizzardAlerts : IBlizzardAlerts
    {
        private readonly IRedisDatabase _redisDatabase;

        public BlizzardAlerts(IRedisDatabase redisDatabase)
        {
            _redisDatabase = redisDatabase;
        }

        public async Task<string> Get(string url)
        {
            if (await _redisDatabase.ExistsAsync($"alert_{url}"))
            {
                return await _redisDatabase.GetAsync<string>($"alert_{url}");
            }

            try
            {
                using var wc = new WebClient();
                var data = await wc.DownloadStringTaskAsync($"http://us.launcher.battle.net/service/{url}/alert/en-us");
                await _redisDatabase.AddAsync($"alert_{url}", data, TimeSpan.FromSeconds(30));

                return data;
            }
            catch
            {
                return string.Empty;
            }
        }
        
        public async Task<string> ExperimentalOnline()
        {
            if (await _redisDatabase.ExistsAsync("alert_pro_experimental_status"))
            {
                return await _redisDatabase.GetAsync<int>("alert_pro_experimental_status") <= 0 ? "Experimental is currently offline please check back later!" : string.Empty;
            }

            var url = $"https://cdn.blz-contentstack.com/v3/content_types/game_update/entries?desc=post_date&environment=prod&query={{\"type\":\"experimental\", \"expired\":false}}&limit=1&locale=en-us";

            var headers = new Dictionary<string, string>
                {
                    { "api_key", "blt43efdd4acc4bdcb2" },
                    { "access_token", "cs10ce60130ad4ae4fcacf3344" }
                };

            var (item1, _) = await BNetLib.Http.RemoteJson.Get<BNetLib.Models.Patchnotes.Overwatch.Root>(url, headers);

            await _redisDatabase.AddAsync("alert_pro_experimental_status", item1.Entries.Count, TimeSpan.FromSeconds(30));

            return item1.Entries.Count <= 0 ? "Experimental is currently offine please check back later!" : string.Empty;
        }
    }
}