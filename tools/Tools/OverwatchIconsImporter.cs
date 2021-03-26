using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;
using Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Minio;
using Newtonsoft.Json;
using Tooling.Attributes;
using Microsoft.Extensions.Configuration;

namespace Tooling.Tools
{
    [Tool(Name = "Overwatch Icon Importer", Order = 1, Disabled = true)]
    public class OverwatchIconsImporter : ITool
    {
        private readonly ILogger<OverwatchIconsImporter> _logger;
        private readonly MinioClient _minioClient;
        private readonly HtmlParser _parser = new();
        private string _bucket;
        private readonly DBContext _dbContext;

        public OverwatchIconsImporter(ILogger<OverwatchIconsImporter> logger, MinioClient minioClient, IConfiguration config, DBContext dbContext)
        {
            _logger = logger;
            _minioClient = minioClient;
            _bucket = config.GetValue("AWS:BucketName", "");
            _dbContext = dbContext;
        }
        
        public async Task Start()
        {
            using var wc = new HttpClient();
            using var wcRes = await wc.GetAsync("http://playoverwatch.com/en-us/search/");
            if (!wcRes.IsSuccessStatusCode) return;
            var rsltContent = await wcRes.Content.ReadAsStringAsync();

            using var doc = await _parser.ParseDocumentAsync(rsltContent);

            var data = doc.QuerySelectorAll("script")
                .FirstOrDefault(x => x.InnerHtml.Contains("window.app.search.init"));

            if (data?.InnerHtml == null) return;

            var jsonData = data.InnerHtml.Replace("window.app.search.init(", "").Replace(");", "");

            var payload = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(jsonData);

            var i = 0;
            foreach (var (key, value) in payload) 
            {
                var fileURL = (string) value.icon;
                var meta = new Dictionary<string, string>
                {
                    ["url"] = fileURL,
                    ["name"] = value.name,
                    ["release"] = value.release.version.ToString(),
                    ["event"] = value.@event.name,
                    ["id"] = key
                };

                var dest = Path.Join("bt", "icons", "overwatch", $"{key}{Path.GetExtension(fileURL)}").Replace("\\", "/").TrimStart('/');

                var exist = await _dbContext.Assets.FirstOrDefaultAsync(x => x.Metadata.RootElement.GetProperty("id").GetString() == key);
                if (exist == null)
                {
                    await _dbContext.Assets.AddAsync(new Assets
                    {
                        url =  $"https://cdn.blizztrack.com/{dest}",
                        Metadata = JsonDocument.Parse(JsonConvert.SerializeObject(meta))
                    });
                    i++;
                }
                else
                {
                    exist.url = $"https://cdn.blizztrack.com/{dest}";
                    exist.Metadata = JsonDocument.Parse(JsonConvert.SerializeObject(meta));
                    _dbContext.Assets.Update(exist);
                    i++;
                }
                
                var response = await wc.GetAsync(fileURL);
                var ms = await response.Content.ReadAsStreamAsync();
                
                await _minioClient.PutObjectAsync(_bucket, dest, ms, ms.Length,
                    $"image/{Path.GetExtension(fileURL).TrimStart('.')}",
                    new Dictionary<string, string> {{"x-amz-acl", "public-read"}});
                
                _logger.LogInformation($"{value.name} => https://cdn.blizztrack.com/{dest}");

                if (i > 9)
                {
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation("Saving changes");
                    i = 0;
                }
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }
            
            await _dbContext.SaveChangesAsync();
        }
    }
}