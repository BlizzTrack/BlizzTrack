using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace BNetLib.Http
{
    public static class RemoteJson
    {
        public static async Task<(T, string)> Get<T>(string url, Dictionary<string, string> headers = null) where T : new()
        {
            using var wc = new WebClient();
            if(headers != null)
            {
               foreach(var header in headers)
               {
                    wc.Headers.Add(header.Key, header.Value);
               }
            }

            var data = await wc.DownloadStringTaskAsync(url);

            return (JsonConvert.DeserializeObject<T>(data), data);
        }
    }
}
