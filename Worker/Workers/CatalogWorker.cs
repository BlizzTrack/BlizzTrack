using BNetLib.Http;
using Core.Models;
using Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Worker.Events;

namespace Worker.Workers
{
    internal class CatalogWorkerHosted : IHostedService
    {
        private readonly IServiceProvider serviceProvider;

        public CatalogWorkerHosted(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(() =>
            {
                var c = ActivatorUtilities.CreateInstance<CatalogWorker>(serviceProvider);
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
        private readonly System.Threading.Channels.ChannelReader<ConfigUpdate> _channel;
        private readonly IServiceScopeFactory _serviceScope;

        public CatalogWorker(ILogger<CatalogWorker> logger, System.Threading.Channels.Channel<ConfigUpdate> channel, IServiceScopeFactory serviceScope)
        {
            _logger = logger;
            _channel = channel;
            _serviceScope = serviceScope;
        }
        internal async void Run(CancellationToken cancellationToken)
        {
            while (await _channel.WaitToReadAsync(cancellationToken))
                while (_channel.TryRead(out ConfigUpdate item))
                {
                    using var sc = _serviceScope.CreateScope();
                    _logger.LogInformation($"Build Manfest Catalog: {item}");

                    if (item.Code == "catalogs")
                    {
                        await CatalogsConfig(item,
                            sc.ServiceProvider.GetRequiredService<ICDNs>(),
                            sc.ServiceProvider.GetRequiredService<ProductConfig>(),
                            sc.ServiceProvider.GetRequiredService<DBContext>()
                        );
                    } 
                    else
                    {
                        await ProductConfig(item,
                            sc.ServiceProvider.GetRequiredService<ProductConfig>(),
                            sc.ServiceProvider.GetRequiredService<DBContext>()
                        );
                    }
                }
        }

        public async Task ProductConfig(ConfigUpdate config, ProductConfig _productConfig, DBContext _dbContext)
        {
            var rootConfig = await _productConfig.GetRaw(config.Hash);
            var json = JsonDocument.Parse(rootConfig);

            if(!await ManifestExist(config.Hash, _dbContext))
            {
                _dbContext.Add(new Catalog()
                {
                    Payload = json,
                    Hash = config.Hash,
                    Name = $"product_config_{config.Code}",
                    Type = CatalogType.ProductConfig,
                });

                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task CatalogsConfig(ConfigUpdate config, ICDNs _cdns, ProductConfig _productConfig, DBContext _dbContext)
        {
            var cdnUrl = await GetCDNUrl(_cdns, config.Code);

            var buildConfig = await _productConfig.GetDictionary(config.Hash, $"{cdnUrl.Hosts.Split(" ").First()}/{cdnUrl.Path}/config");

            var hash = buildConfig["root"];
            var rootConfig = await _productConfig.GetRaw(hash, $"{cdnUrl.Hosts.Split(" ").First()}/{cdnUrl.Path}/data");
            var json = JsonDocument.Parse(rootConfig);

            var fragments = new List<BNetLib.Catalogs.Models.Fragment>();

            if (!await ManifestExist(buildConfig["root"], _dbContext))
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

                _dbContext.Add(new Catalog()
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

            foreach (var item in fragments)
            {
                if (!await ManifestExist(item.hash, _dbContext))
                {
                    _logger.LogInformation($"Missing (Inserting): {item.name} {item.hash}");

                    var gameConfig = await _productConfig.GetRaw(item.hash, $"{cdnUrl.Hosts.Split(" ").First()}/{cdnUrl.Path}/data");
                    var gameJson = JsonDocument.Parse(gameConfig);


                    var installs = new List<CatalogInstall>();
                    var strings = new Dictionary<string, string>();

                    var is_Act = false;
                    var translationName = string.Empty;
                    if (gameJson.RootElement.TryGetProperty("products", out var productsJson))
                    {
                        JsonElement baseItem = new JsonElement();

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
                                is_Act = f.GetBoolean();
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
                        Hash = item.hash,
                        Name = item.name,
                        Type = CatalogType.Fragment,
                        Activision = is_Act,
                        Installs = installs,
                        Translations = strings,
                        ProperName = translationName == string.Empty ? string.Empty : strings[translationName]
                    });
                }

                _logger.LogInformation($"Processed: {item.name} {item.hash}");
            }

            _logger.LogInformation($"Saved fragment data");
            await _dbContext.SaveChangesAsync();
        }

        private async Task<BNetLib.Models.CDN> GetCDNUrl(ICDNs _cdns, string code)
        {
            var latestCatalogCDN = await _cdns.Latest(code);

            return latestCatalogCDN.Content.FirstOrDefault(x => x.Name.Equals("us", StringComparison.OrdinalIgnoreCase));
        }

        private async Task<bool> ManifestExist(string hash, DBContext _dbContext)
        {
            var exist = await _dbContext.Catalogs.FirstOrDefaultAsync(x => x.Hash == hash);

            if (exist == null) return false;
            return true;
        }
    }
}
