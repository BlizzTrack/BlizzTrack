using BNetLib.Http;
using BNetLib.Networking;
using BNetLib.Networking.Commands;
using Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Worker.Workers
{
    class SummaryHosted : IHostedService
    {
        private readonly IServiceProvider serviceProvider;

        public SummaryHosted(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(() => {
                var c = ActivatorUtilities.CreateInstance<Summary>(serviceProvider);
                c.Run(cancellationToken);
            }, cancellationToken);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    class Summary
    {
        private readonly BNetClient _bNetClient;
        private readonly ProductConfig _productConfig;
        private readonly ILogger<Summary> _logger;
        private readonly IServiceScopeFactory _serviceScope;

        public Summary(BNetClient bNetClient, ProductConfig productConfig, ILogger<Summary> logger, IServiceScopeFactory scopeFactory)
        {
            _bNetClient = bNetClient;
            _productConfig = productConfig;
            _logger = logger;
            _serviceScope = scopeFactory;
        }

        public async void Run(CancellationToken cancellationToken)
        {
            bool firstRun = true;
            while (!cancellationToken.IsCancellationRequested)
            {
                using var sc = _serviceScope.CreateScope();
                var _summary = sc.ServiceProvider.GetRequiredService<Core.Services.ISummary>();
                var _dbContext = sc.ServiceProvider.GetRequiredService<DBContext>();

                var latest = await _summary.Latest();
                if (firstRun)
                {
                    foreach (var item in latest.Content)
                    {
                        await AddItemToData(item, _dbContext, cancellationToken);
                        _dbContext.SaveChanges();
                    }

                    foreach (var item in latest.Content)
                    {
                        try
                        {
                            await CheckifEncrypted(item, _dbContext, cancellationToken);
                            _dbContext.SaveChanges();
                        }catch
                        {
                            // ignore
                        }
                    }

                    firstRun = false;
                }

                (var summary, int seqn) = await _bNetClient.Do<List<BNetLib.Models.Summary>>(new SummaryCommand());
                if (latest?.Seqn < seqn)
                {
                    latest = Manifest<BNetLib.Models.Summary[]>.Create(seqn, "summary", summary.ToArray());
                    try
                    {
                        _dbContext.Summary.Add(latest);
                        _dbContext.SaveChanges();
                    }
                    catch(Exception ex)
                    {
                        _logger.LogCritical($"Failed to write summary: {ex}");
                    }

                    var latestItems = latest?.Content.ToList() ?? new List<BNetLib.Models.Summary>();
                    foreach (var item in summary)
                    {
                        try
                        {
                            await AddItemToData(item, _dbContext, cancellationToken);
                            _dbContext.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogCritical($"Failed to write {item.Product}-{item.Flags}-{item.Seqn}: {ex}");
                        }
                    }

                    foreach (var item in summary)
                    {
                        try
                        {
                            await CheckifEncrypted(item, _dbContext, cancellationToken);
                            _dbContext.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogCritical($"Failed to write {item.Product}-config-{item.Seqn}: {ex}");
                        }

                    }

                }
                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
            }
        }

        private async Task AddItemToData(BNetLib.Models.Summary msg, DBContext db, CancellationToken cancellationToken)
        {
            var code = msg.Product;

            switch (msg.Flags)
            {
                case "versions" or "version":
                    {
                        var exist = await db.Versions.AsNoTracking().OrderByDescending(x => x.Seqn).FirstOrDefaultAsync(x => x.Code == code && x.Seqn == msg.Seqn, cancellationToken: cancellationToken);

                        if (exist != null)
                        {
                            _logger.LogInformation($"Skipping {code}:{msg.Seqn}:{msg.Flags}");
                            return;
                        }
                        break;
                    }
                case "cdn" or "cdns":
                    {
                        var exist = await db.CDN.AsNoTracking().OrderByDescending(x => x.Seqn).FirstOrDefaultAsync(x => x.Code == code && x.Seqn == msg.Seqn, cancellationToken: cancellationToken);

                        if (exist != null)
                        {
                            _logger.LogInformation($"Skipping {code}:{msg.Seqn}:{msg.Flags}");
                            return;
                        }
                        break;
                    }
                case "bgdl":
                    {
                        var exist = await db.BGDL.AsNoTracking().OrderByDescending(x => x.Seqn).FirstOrDefaultAsync(x => x.Code == code && x.Seqn == msg.Seqn, cancellationToken: cancellationToken);

                        if (exist != null)
                        {
                            _logger.LogInformation($"Skipping {code}:{msg.Seqn}:{msg.Flags}");
                            return;
                        }
                        break;
                    }
            }

            _logger.LogInformation($"We didn't skip: {code}-{msg.Seqn}-{msg.Flags}");

            var (data, seqn) = await GetMetaData(msg);

            switch (data)
            {
                case List<BNetLib.Models.Versions> ver:
                    {
                        var f = Manifest<BNetLib.Models.Versions[]>.Create(seqn, code, ver.ToArray());
                        db.Versions.Add(f);
                        break;
                    }
                case List<BNetLib.Models.CDN> cdn:
                    {
                        var f = Manifest<BNetLib.Models.CDN[]>.Create(seqn, code, cdn.ToArray());
                        db.CDN.Add(f);
                        break;
                    }
                case List<BNetLib.Models.BGDL> bgdl:
                    {
                        var f = Manifest<BNetLib.Models.BGDL[]>.Create(seqn, code, bgdl.ToArray());
                        db.BGDL.Add(f);
                        break;
                    }
                default:
                    _logger.LogCritical("Unhandled type");
                    break;
            }
        }

        private async Task<(IList data, int seqn)> GetMetaData(BNetLib.Models.Summary msg)
        {
            (IList data, int seqn) res = msg.Flags switch
            {
                "version" or "versions" => await _bNetClient.Do<List<BNetLib.Models.Versions>>(new VersionCommand(msg.Product)),
                "cdn" or "cdns" => await _bNetClient.Do<List<BNetLib.Models.CDN>>(new CDNCommand(msg.Product)),
                "bgdl" => await _bNetClient.Do<List<BNetLib.Models.BGDL>>(new BGDLCommand(msg.Product)),
                _ => (null, -1)
            };

            if (res.seqn != msg.Seqn)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                return await GetMetaData(msg);
            }

            return (res.data, res.seqn);
        }

        private async Task CheckifEncrypted(BNetLib.Models.Summary msg, DBContext dbContext, CancellationToken cancellationToken)
        {
            if (msg.Flags == "cdn" || msg.Flags == "bgdl") return; 
            var versionConfig = await dbContext.Versions.AsNoTracking().OrderByDescending(x => x.Seqn).FirstOrDefaultAsync(x => x.Code == msg.Product, cancellationToken: cancellationToken);

            var config = versionConfig.Content.FirstOrDefault(x => x.Region == "us");
            if (msg.Product == "catalogs") config = versionConfig.Content.Last();
            if (config == null) return;

            var file = await _productConfig.GetRaw(config.Productconfig);

            var currentGameConfig = await dbContext.GameConfigs.FirstOrDefaultAsync(x => x.Code == msg.Product, cancellationToken: cancellationToken);

            if (file.Contains("\"decryption_key_name\""))
            {
                if(currentGameConfig == null)
                {
                    await dbContext.GameConfigs.AddAsync(new GameConfig()
                    {
                        Code = msg.Product,
                        Config = new ConfigItems()
                        {
                            Encrypted = true,
                        }
                    }, cancellationToken);
                } else
                {
                    currentGameConfig.Config.Encrypted = true;
                    dbContext.GameConfigs.Update(currentGameConfig);
                }
            }  else
            {
                if (currentGameConfig == null)
                {
                    await dbContext.GameConfigs.AddAsync(new GameConfig()
                    {
                        Code = msg.Product,
                        Config = new ConfigItems()
                        {
                            Encrypted = false,
                        }
                    }, cancellationToken);
                }
                else
                {
                    currentGameConfig.Config.Encrypted = false;
                    dbContext.GameConfigs.Update(currentGameConfig);
                }
            }
        }
    }
}
