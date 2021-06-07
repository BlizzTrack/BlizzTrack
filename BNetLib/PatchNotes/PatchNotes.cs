using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using BNetLib.Http;
using BNetLib.PatchNotes.Models;
using Newtonsoft.Json;

namespace BNetLib.PatchNotes
{
    public interface IPatchNotes
    {
        Task<(T parsed, string raw)> Get<T>(string url, Dictionary<string, string> headers = null);
    }
    
    public class PatchNotes : IPatchNotes
    {
        public async Task<(T parsed, string raw)> Get<T>(string url, Dictionary<string, string> headers = null)
        {
            using var wc = new WebClient();
            if (headers != null)
            {
                foreach (var (key, value) in headers)
                {
                    wc.Headers.Add(key, value);
                }
            }

            var data = await wc.DownloadStringTaskAsync(url);

            return (JsonConvert.DeserializeObject<T>(data), data);
        }
    }
}