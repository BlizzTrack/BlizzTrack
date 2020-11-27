using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace BNetLib.Http
{
    public class ProductConfig
    {
        public async Task<string> GetRaw(string config, string path = "level3.blizzard.com/tpr/configs/data")
        {
            using var wc = new WebClient();

            var url = $"http://{path}/{string.Join("", config.Take(2))}/{string.Join("", config.Skip(2).Take(2))}/{config}";

            return await wc.DownloadStringTaskAsync(url);
        }

        public async Task<Dictionary<string, string>> GetDictionary(string config, string path = "level3.blizzard.com/tpr/configs/data")
        {
            var data = await GetRaw(config, path);

            var res = new Dictionary<string, string>();

            foreach (var line in data.Split("\n"))
            {
                var l = line.Trim();

                if (l.StartsWith("#") || string.IsNullOrEmpty(l)) continue;
                var lineData = l.Split(" = ");
                res.Add(lineData.First(), lineData.Last());
            }

            return res;
        }
    }
}
