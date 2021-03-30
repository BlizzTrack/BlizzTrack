using BNetLib.Catalogs;
using Core.Models;
using Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.Exceptions;
using ShellProgressBar;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Tooling.Attributes;

namespace Tooling.Tools
{
    [Tool(Name = "Catalogs Import", Disabled = true)]
    public class CatalogsImport : ITool
    {
        private readonly IVersions _versions;
        private readonly ICDNs _cdns;
        private readonly DBContext _dbContext;
        private readonly ILogger<CatalogsImport> _logger;

        public CatalogsImport(IVersions versions, ICDNs cdns, ILogger<CatalogsImport> logger, DBContext dbContext)
        {
            _versions = versions;
            _cdns = cdns;
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task Start()
        {
            // await Catalogs();
            // await ProductConfig();

            await BuildConfig();
        }

        protected async Task BuildConfig()
        {
            Console.Clear();
            
            var versions = await _dbContext.Versions.OrderByDescending(x => x.Seqn).ToListAsync();
            var cdns = await _dbContext.CDN.OrderByDescending(x => x.Seqn).ToListAsync();

            var used = new List<string>();

            var configs = await _dbContext.GameConfigs.ToListAsync();

            int i = 1;
            foreach (var version in versions)
            {
                var cdn = cdns.FirstOrDefault(x => x.Code == version.Code);
                if (cdn == null) continue;

                // _logger.LogInformation($"Testing: {version.Code} {version.Seqn}");

                var cdnRegion = cdn.Content.FirstOrDefault(x => x.Name == "us");
                var gConfig = configs.FirstOrDefault(x => x.Code == version.Code);

                var server = cdnRegion.Hosts.Split(" ").First();

                foreach (var region in version.Content)
                {
                    var config = region.Buildconfig;
                    var p = $"{server}/{cdnRegion.Path}/config";
                    var dest = $"{cdnRegion.Path}/config/{string.Join("", config.Take(2))}/{string.Join("", config.Skip(2).Take(2))}/{config}";
                    if (used.Contains(dest)) continue;
                    if (await ManifestExist(dest)) continue;

                    try
                    {
                        var rootConfig = await BNetLib.Http.ProductConfig.GetBytes(region.Buildconfig, p);

                        /*
                        using var ms = GenerateStreamFromString(rootConfig);

                        await _minioClient.PutObjectAsync(_bucket, dest, ms, ms.Length, gConfig.Config.Encrypted ? "application/octet-stream" : "text/plain", new Dictionary<string, string> {
                            { "x-amz-acl", "public-read" },
                            { "hash", region.Buildconfig },
                            { "code", version.Code },
                            { "region", region.Region },
                            { "server", server },
                            { "remote", $"http://{server}/{dest}" }
                        });
                        */

                        var data = new BuildConfigItem
                        {
                            Content = gConfig != null && gConfig.Config.Encrypted ? Convert.ToBase64String(rootConfig) : System.Text.Encoding.Default.GetString(rootConfig),
                            URL = $"{server}/{dest}",
                            Encrypted = gConfig.Config.Encrypted,
                            Meta = new Dictionary<string, string> {
                                { "hash", region.Buildconfig },
                                { "code", version.Code },
                                { "region", region.Region },
                            }
                        };

                        var d = JsonDocument.Parse(data.ToString());
                        _dbContext.Add(new Core.Models.Catalog()
                        {
                            Payload = d,
                            Hash = dest,
                            Name = $"build_config_{version.Code}",
                            Type = CatalogType.BuildConfig,
                        });


                        _logger.LogInformation($"Saved {version.Code} config {dest}");

                        used.Add(dest);

                        i++;
                    }
                    catch (MinioException m)
                    {
                        _logger.LogError(m.ToString());
                        Debugger.Break();
                    }
                    catch (Exception ex)
                    {
                        used.Add(dest);

                        _logger.LogError($"URL: {server}/{dest}");
                        _logger.LogError(ex.ToString());
                    }
                }

                if (i < 10) continue;
                i = 1;
                _logger.LogInformation("Triggering Saving... Please wait");
                await _dbContext.SaveChangesAsync();
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task ProductConfig()
        {
            Console.Clear();

            var options = new ProgressBarOptions
            {
                ForegroundColor = ConsoleColor.Yellow,
                ForegroundColorDone = ConsoleColor.DarkGreen,
                BackgroundColor = ConsoleColor.DarkGray,
                BackgroundCharacter = '\u2593',
                CollapseWhenFinished = true
            };
            var childOptions = new ProgressBarOptions
            {
                ForegroundColor = ConsoleColor.Green,
                BackgroundColor = ConsoleColor.DarkGreen,
                CollapseWhenFinished = true,
                BackgroundCharacter = '\u2593',
            };

            var versions = await _dbContext.Versions.Where(x => x.Code != "catalogs").OrderBy(x => x.Seqn).ToListAsync();
            // versions = versions.GroupBy(x => x.Code).Select(x => x.OrderByDescending(s => s.Seqn).First()).ToList();

            var used = new List<string>();

            using var pbar = new ProgressBar(versions.Count, "main progressbar", options);

            int i = 1;
            foreach(var version in  versions)
            {
                pbar.Message = $"Importing code: {version.Code} {version.Seqn}";

               // var d = pbar.Spawn(version.Content.Length, "Regions");
                foreach (var region in version.Content)
                {
                    // d.Message = $"Importing: {version.Code} {region.Region} {region.Productconfig}";

                    try
                    {
                        if (!await ManifestExist(region.Productconfig))
                        {
                            var rootConfig = await BNetLib.Http.ProductConfig.GetRaw(region.Productconfig);
                            var json = JsonDocument.Parse(rootConfig);

                            if (used.Contains(region.Productconfig)) continue;
                                
                            _dbContext.Add(new Core.Models.Catalog()
                            {
                                Payload = json,
                                Hash = region.Productconfig,
                                Name = $"product_config_{version.Code}",
                                Type = CatalogType.ProductConfig,
                            });

                            used.Add(region.Productconfig);
                            i++;
                        }
                    }
                    catch
                    {
                        // _logger.LogError($"Failed to download product config for: {version.Code} -> {region.Productconfig}");
                    }

                    // d.Tick();
                }

                if (i == 10)
                {
                    await _dbContext.SaveChangesAsync();
                    i = 1;
                }

                pbar.Tick();
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task Catalogs()
        {
            var latestCatalogVersion = await _versions.Latest("catalogs");
            var latestCatalogCdn = await _cdns.Latest("catalogs");

            var usCdnConfig = latestCatalogCdn.Content.FirstOrDefault(x => x.Name.Equals("us", StringComparison.OrdinalIgnoreCase));
            var latest = latestCatalogVersion.Content.Last();

            //var configFile = await _catalogs.Get(usCDNConfig.Hosts.Split(" ").First(), usCDNConfig.Path, CatalogDataType.Config, latest.Buildconfig);

            var buildConfig = await BNetLib.Http.ProductConfig.GetDictionary(latest.Buildconfig, $"{usCdnConfig.Hosts.Split(" ").First()}/{usCdnConfig.Path}/config");

            var hash = buildConfig["root"];
            var rootConfig = await BNetLib.Http.ProductConfig.GetRaw(hash, $"{usCdnConfig.Hosts.Split(" ").First()}/{usCdnConfig.Path}/data");
            var json = JsonDocument.Parse(rootConfig);

            var fragments = new List<BNetLib.Catalogs.Models.Fragment>();

            if(!await ManifestExist(buildConfig["root"]))
            {
                var s = new Dictionary<string, string>();
                if (json.RootElement.TryGetProperty("strings", out var gameStrings))
                {
                    if (gameStrings.TryGetProperty("default", out var defaultString))
                    {
                        var t = defaultString.EnumerateObject();
                        while (t.MoveNext())
                        {
                            s[t.Current.Name] = t.Current.Value.GetString();
                        }
                    }
                }

                _dbContext.Add(new Core.Models.Catalog()
                {
                    Payload = json,
                    Hash = hash,
                    Name = "root_catalogs",
                    Type = CatalogType.Config,
                    Translations = s
                });
            }

            var pageFragments = json.RootElement.GetProperty("fragments").EnumerateArray();

            _logger.LogInformation("Loading Fragment IDs...");

            while (pageFragments.MoveNext())
            {
                var current = pageFragments.Current;

                fragments.Add(new BNetLib.Catalogs.Models.Fragment(current.GetProperty("name").GetString(), current.GetProperty("hash").GetString()));
            }

            _logger.LogInformation($"Fragments: {string.Join(", ", fragments.Select(x => x.name))}");

            foreach(var (name, s) in fragments)
            {
                if(!await ManifestExist(s))
                {
                    _logger.LogInformation($"Missing (Inserting): {name} {s}");

                    var gameConfig = await BNetLib.Http.ProductConfig.GetRaw(s, $"{usCdnConfig.Hosts.Split(" ").First()}/{usCdnConfig.Path}/data");
                    var gameJson = JsonDocument.Parse(gameConfig);


                    var installs = new List<CatalogInstall>();
                    var strings = new Dictionary<string, string>();

                    var isAct = false;
                    var translationName = string.Empty;
                    var baseItem = new JsonElement();
                    if (gameJson.RootElement.TryGetProperty("products", out var productsJson))
                    {
                        var products = productsJson.EnumerateArray();

                        while (products.MoveNext())
                        {
                            if (products.Current.TryGetProperty("base", out baseItem))
                            {
                                break;
                            }
                        }

                        if (baseItem.ValueKind != JsonValueKind.Null)
                        {
                            if (baseItem.TryGetProperty("is_activision_game", out var f))
                            {
                                isAct = f.GetBoolean();
                            }

                            if (baseItem.TryGetProperty("name", out var n))
                            {
                                translationName = n.GetString();
                            }
                        }
                    }

                    if(gameJson.RootElement.TryGetProperty("installs", out var gameInstalls))
                    {
                        var t = gameInstalls.EnumerateObject();

                        while(t.MoveNext())
                        {
                            installs.Add(new CatalogInstall()
                            {
                                Name = t.Current.Name,
                                Code = t.Current.Value.GetProperty("tact_product").GetString()
                            });
                        }
                    }

                    if (gameJson.RootElement.TryGetProperty("strings", out var gameStrings))
                    {
                        if (gameStrings.TryGetProperty("default", out var defaultString))
                        {
                            var t = defaultString.EnumerateObject();
                            while (t.MoveNext())
                            {
                                strings[t.Current.Name] = t.Current.Value.GetString();
                            }
                        }
                    }

                    _dbContext.Add(new Core.Models.Catalog()
                    {
                        Payload = gameJson,
                        Hash = s,
                        Name = name,
                        Type = CatalogType.Fragment,
                        Activision = isAct,
                        Installs = installs,
                        Translations = strings,
                        ProperName = translationName == string.Empty ? string.Empty : strings[translationName]
                    });
                }

                _logger.LogInformation($"Processed: {name} {s}");
            }

            _logger.LogInformation($"Saved fragment data");
            await _dbContext.SaveChangesAsync();
        }

        private async Task<bool> ManifestExist(string hash)
        {
            var exist = await _dbContext.Catalogs.FirstOrDefaultAsync(x => x.Hash == hash);

            if (exist == null) return false;
            return true;
        }
    }
}
