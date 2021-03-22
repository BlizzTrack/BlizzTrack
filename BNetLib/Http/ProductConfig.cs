using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace BNetLib.Http
{
    public class ProductConfig
    {
        public static async Task<string> GetRaw(string config, string path = "level3.blizzard.com/tpr/configs/data")
        {
            using var wc = new WebClient();

            var url = $"http://{path}/{string.Join("", config.Take(2))}/{string.Join("", config.Skip(2).Take(2))}/{config}";

            return await wc.DownloadStringTaskAsync(url);
        }

        public static async Task<byte[]> GetBytes(string config, string path = "level3.blizzard.com/tpr/configs/data")
        {
            using var wc = new WebClient();

            var url = $"http://{path}/{string.Join("", config.Take(2))}/{string.Join("", config.Skip(2).Take(2))}/{config}";

            return await wc.DownloadDataTaskAsync(url);
        }

        public static async Task<Dictionary<string, string>> GetDictionary(string config,
            string path = "level3.blizzard.com/tpr/configs/data")
        {
            var data = await GetRaw(config, path);

            return (from line in data.Split("\n")
                select line.Trim()
                into l
                where !l.StartsWith("#") && !string.IsNullOrEmpty(l)
                select l.Split(" = ")).ToDictionary(lineData => lineData.First(), lineData => lineData.Last());
        }
    }
}