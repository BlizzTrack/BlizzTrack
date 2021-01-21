﻿using BNetLib.Catalogs;
using BNetLib.Http;
using Core.Models;
using Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Tooling.Attributes;

namespace Tooling.Tools
{
    [Tool(Name = "Catalogs Import")]
    public class CatalogsImport : ITool
    {
        private readonly IVersions _versions;
        private readonly ICDNs _cdns;
        private readonly ProductConfig _productConfig;
        private readonly DBContext _dbContext;
        private readonly ILogger<CatalogsImport> _logger;

        public CatalogsImport(IVersions versions, ICDNs cdns, ProductConfig productConfig, ILogger<CatalogsImport> logger, DBContext dbContext)
        {
            _versions = versions;
            _cdns = cdns;
            _productConfig = productConfig;
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task Start()
        {
            await ProductConfig();
        }

        public async Task ProductConfig()
        {
            var versions = await _dbContext.Versions.OrderByDescending(x => x.Seqn).ToListAsync();
            versions = versions.GroupBy(x => x.Code).Select(x => x.OrderByDescending(s => s.Seqn).First()).ToList();

            foreach(var version in  versions)
            {
                var hash = version.Content.FirstOrDefault(x => x.Region.Equals("us", StringComparison.OrdinalIgnoreCase));
                if (hash == null) continue;

                try
                {
                    var rootConfig = await _productConfig.GetRaw(hash.Productconfig);
                    var json = JsonDocument.Parse(rootConfig);

                    if (!await ManifestExist(hash.Productconfig))
                    {
                        _dbContext.Add(new Catalog()
                        {
                            Payload = json,
                            Hash = hash.Productconfig,
                            Name = $"product_config_{version.Code}",
                            Type = CatalogType.ProductConfig,
                        });

                    }
                }catch
                {
                    _logger.LogError($"Failed to download product config for: {version.Code} -> {hash.Productconfig}");
                    continue;
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task Catalogs()
        {
            var latestCatalogVersion = await _versions.Latest("catalogs");
            var latestCatalogCDN = await _cdns.Latest("catalogs");

            var usCDNConfig = latestCatalogCDN.Content.FirstOrDefault(x => x.Name.Equals("us", StringComparison.OrdinalIgnoreCase));
            var latest = latestCatalogVersion.Content.Last();

            //var configFile = await _catalogs.Get(usCDNConfig.Hosts.Split(" ").First(), usCDNConfig.Path, CatalogDataType.Config, latest.Buildconfig);

            var buildConfig = await _productConfig.GetDictionary(latest.Buildconfig, $"{usCDNConfig.Hosts.Split(" ").First()}/{usCDNConfig.Path}/config");

            var hash = buildConfig["root"];
            var rootConfig = await _productConfig.GetRaw(hash, $"{usCDNConfig.Hosts.Split(" ").First()}/{usCDNConfig.Path}/data");
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

            foreach(var item in fragments)
            {
                if(!await ManifestExist(item.hash))
                {
                    _logger.LogInformation($"Missing (Inserting): {item.name} {item.hash}");

                    var gameConfig = await _productConfig.GetRaw(item.hash, $"{usCDNConfig.Hosts.Split(" ").First()}/{usCDNConfig.Path}/data");
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

                    _dbContext.Add(new Catalog()
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

        private async Task<bool> ManifestExist(string hash)
        {
            var exist = await _dbContext.Catalogs.FirstOrDefaultAsync(x => x.Hash == hash);

            if (exist == null) return false;
            return true;
        }
    }
}