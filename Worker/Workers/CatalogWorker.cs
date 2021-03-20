using Core.Models;
using Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Worker.Events;

namespace Worker.Workers
{
    internal class CatalogWorkerHosted : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public CatalogWorkerHosted(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(() =>
            {
                var c = ActivatorUtilities.CreateInstance<CatalogWorker>(_serviceProvider);
                c.Run(cancellationToken);
            }, cancellationToken);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
    public class CatalogWorker
    {
        private readonly ILogger<CatalogWorker> _logger;
        private readonly ConcurrentQueue<ConfigUpdate> _channel;
        private readonly IServiceScopeFactory _serviceScope;

        public CatalogWorker(ILogger<CatalogWorker> logger, ConcurrentQueue<ConfigUpdate> channel, IServiceScopeFactory serviceScope)
        {
            _logger = logger;
            _channel = channel;
            _serviceScope = serviceScope;
        }
        internal async void Run(CancellationToken cancellationToken)
        {
          
            while(!cancellationToken.IsCancellationRequested)
            {
                if(_channel.TryDequeue(out var item))
                {
                    using var sc = _serviceScope.CreateScope();
                    _logger.LogInformation($"Build Manfest Catalog: {item.Code} {item.Hash}");

                    if (item.Code == "catalogs")
                    {
                        _logger.LogInformation($"Build Config: {item.Code} {item.Hash}");

                        await CatalogsConfig(item,
                            sc.ServiceProvider.GetRequiredService<ICDNs>(),
                            sc.ServiceProvider.GetRequiredService<DBContext>()
                        );
                    }
                    else
                    {
                        _logger.LogInformation($"Product Config: {item.Code} {item.Hash}");

                        await ProductConfig(item,
                            sc.ServiceProvider.GetRequiredService<DBContext>()
                        );
                    }
                }

                await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken);
            }

            _logger.LogCritical("Some how we got here for CatalogWorker");
        }

        private async Task ProductConfig(ConfigUpdate config, DBContext dbContext)
        {
           
            if(!await ManifestExist(config.Hash, dbContext))
            {
                var json = await GetRaw(config.Hash);

                dbContext.Add(new Core.Models.Catalog()
                {
                    Payload = json,
                    Hash = config.Hash,
                    Name = $"product_config_{config.Code}",
                    Type = CatalogType.ProductConfig,
                });

                await dbContext.SaveChangesAsync();
            }
        }

        private async Task CatalogsConfig(ConfigUpdate config, ICDNs cdns, DBContext dbContext)
        {
            var cdnUrl = await GetCdnUrl(cdns, config.Code);

            var buildConfig = await BNetLib.Http.ProductConfig.GetDictionary(config.Hash, $"{cdnUrl.Hosts.Split(" ").First()}/{cdnUrl.Path}/config");

            var hash = buildConfig["root"];
            
            var fragments = new List<BNetLib.Catalogs.Models.Fragment>();

            if (!await ManifestExist(buildConfig["root"], dbContext))
            {
                var json = await GetRaw(hash, $"{cdnUrl.Hosts.Split(" ").First()}/{cdnUrl.Path}/data");

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

                dbContext.Add(new Core.Models.Catalog()
                {
                    Payload = json,
                    Hash = hash,
                    Name = "root_catalogs",
                    Type = CatalogType.Config,
                    Translations = s
                });

                var pageFragments = json.RootElement.GetProperty("fragments").EnumerateArray();

                _logger.LogInformation("Loading Fragment IDs...");

                while (pageFragments.MoveNext())
                {
                    var current = pageFragments.Current;

                    fragments.Add(new BNetLib.Catalogs.Models.Fragment(current.GetProperty("name").GetString(), current.GetProperty("hash").GetString()));
                }

                _logger.LogInformation($"Fragments: {string.Join(", ", fragments.Select(x => x.name))}");

                foreach (var item in fragments)
                {
                    if (!await ManifestExist(item.hash, dbContext))
                    {
                        _logger.LogInformation($"Missing (Inserting): {item.name} {item.hash}");

                        var gameJson = await GetRaw(item.hash, $"{cdnUrl.Hosts.Split(" ").First()}/{cdnUrl.Path}/data");

                        var installs = new List<CatalogInstall>();
                        var strings = new Dictionary<string, string>();

                        var isAct = false;
                        var translationName = string.Empty;
                        if (gameJson.RootElement.TryGetProperty("products", out var productsJson))
                        {
                            var baseItem = new JsonElement();

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

                        if (gameJson.RootElement.TryGetProperty("installs", out var gameInstalls))
                        {
                            var t = gameInstalls.EnumerateObject();

                            while (t.MoveNext())
                            {
                                installs.Add(new CatalogInstall()
                                {
                                    Name = t.Current.Name,
                                    Code = t.Current.Value.GetProperty("tact_product").GetString()
                                });
                            }
                        }

                        if (gameJson.RootElement.TryGetProperty("strings", out var gss))
                        {
                            if (gss.TryGetProperty("default", out var defaultString))
                            {
                                var t = defaultString.EnumerateObject();
                                while (t.MoveNext())
                                {
                                    strings[t.Current.Name] = t.Current.Value.GetString();
                                }
                            }
                        }

                        dbContext.Add(new Core.Models.Catalog()
                        {
                            Payload = gameJson,
                            Hash = item.hash,
                            Name = item.name,
                            Type = CatalogType.Fragment,
                            Activision = isAct,
                            Installs = installs,
                            Translations = strings,
                            ProperName = translationName == string.Empty ? string.Empty : strings[translationName]
                        });
                    }

                    _logger.LogInformation($"Processed: {item.name} {item.hash}");
                }

                _logger.LogInformation($"Saved fragment data");
                await dbContext.SaveChangesAsync();
            }
        }

        private async Task<BNetLib.Models.CDN> GetCdnUrl(ICDNs cdns, string code)
        {
            var latestCatalogCdn = await cdns.Latest(code);

            return latestCatalogCdn.Content.FirstOrDefault(x => x.Name.Equals("us", StringComparison.OrdinalIgnoreCase));
        }

        private async Task<bool> ManifestExist(string hash, DBContext dbContext)
        {
            var exist = await dbContext.Catalogs.FirstOrDefaultAsync(x => x.Hash == hash);

            if (exist == null) return false;
            return true;
        }

        private async Task<JsonDocument> GetRaw(string hash, string url = "level3.blizzard.com/tpr/configs/data", int count = 1)
        {
            if (count == 5)
            {
                var f = $"http://{url}/{string.Join("", hash.Take(2))}/{string.Join("", hash.Skip(2).Take(2))}/{hash}";

                _logger.LogCritical($"Hit retry limit: {f}");
                return null;
            }
            try
            {
                var gameConfig = await BNetLib.Http.ProductConfig.GetRaw(hash, url);
                var gameJson = JsonDocument.Parse(gameConfig);


                return gameJson;
            }catch
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                return await GetRaw(hash, url, ++count);
            }
        }
    }
}
