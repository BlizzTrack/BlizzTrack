using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BNetLib.Catalogs
{
    public enum CatalogDataType
    {
        Config,
        Data
    }

    public interface ICatalogs
    {
        Task<string> Get(string cdnBase, string cdnPath, CatalogDataType type, string hash);
    }
    
    public class Catalogs : ICatalogs
    {
        public async Task<string> Get(string cdnBase, string cdnPath, CatalogDataType type, string hash)
        {
            using var wc = new WebClient();

            var url = $"http://{cdnBase}/{cdnPath}/{type.ToString().ToLower()}/{string.Join("", hash.Take(2))}/{string.Join("", hash.Skip(2).Take(2))}/{hash}";

            var data = await wc.DownloadDataTaskAsync(url);

            return Encoding.Default.GetString(data);
        }
    }
}
