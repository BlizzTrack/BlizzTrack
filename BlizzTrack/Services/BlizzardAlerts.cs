using StackExchange.Redis.Extensions.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace BlizzTrack.Services
{
    public interface IBlizzardAlerts
    {
        bool Has(string code);

        Task<string> Get(string code);
    }

    public class BlizzardAlerts : IBlizzardAlerts
    {
        private readonly IRedisDatabase _redisDatabase;

        public BlizzardAlerts(IRedisDatabase redisDatabase)
        {
            _redisDatabase = redisDatabase;
        }

        // code => url
        private readonly Dictionary<string, string> _games = new Dictionary<string, string>()
        {
            { "pro", "overwatch" },
            { "prot", "overwatch/ptr" }
        };

        public async Task<string> Get(string url)
        {
            if(await _redisDatabase.ExistsAsync($"alert_{url}"))
            {
                return await _redisDatabase.GetAsync<string>($"alert_{url}");
            }

            try
            {
                using var wc = new WebClient();
                var data = await wc.DownloadStringTaskAsync($"http://us.launcher.battle.net/service/{url}/alert/en-us");
                await _redisDatabase.AddAsync($"alert_{url}", data, TimeSpan.FromSeconds(30));

                return data;
            }catch
            {
                return string.Empty;
            }
        }

        public bool Has(string code)
        {
            return _games.ContainsKey(code);
        }
    }
}
