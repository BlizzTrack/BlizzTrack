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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Worker.Events;

namespace Worker.Workers
{
    internal class SummaryHosted : IHostedService
    {
        private readonly IServiceProvider serviceProvider;

        public SummaryHosted(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(() =>
            {
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

    internal class Summary
    {
        private readonly BNetClient _bNetClient;
        private readonly ProductConfig _productConfig;
        private readonly ILogger<Summary> _logger;
        private readonly IServiceScopeFactory _serviceScope;
        private readonly System.Threading.Channels.ChannelWriter<ConfigUpdate> _channelWriter;

        public Summary(BNetClient bNetClient, ProductConfig productConfig, ILogger<Summary> logger, IServiceScopeFactory scopeFactory, System.Threading.Channels.ChannelWriter<ConfigUpdate> channelWriter)
        {
            _bNetClient = bNetClient;
            _productConfig = productConfig;
            _logger = logger;
            _serviceScope = scopeFactory;
            _channelWriter = channelWriter;
        }

        public async void Run(CancellationToken cancellationToken)
        {
            bool firstRun = true;
            while (!cancellationToken.IsCancellationRequested)
            {
                var stopWatch = Stopwatch.StartNew();

                using var sc = _serviceScope.CreateScope();
                var _summary = sc.ServiceProvider.GetRequiredService<Core.Services.ISummary>();
                var _dbContext = sc.ServiceProvider.GetRequiredService<DBContext>();

                var latest = await _summary.Latest();
                if (firstRun)
                {
                    foreach (var item in latest.Content)
                    {
                        await AddItemToData(item, latest.Seqn, _dbContext, cancellationToken);
                        _dbContext.SaveChanges();
                    }

                    firstRun = false;
                }

                (var summary, int seqn, string raw) = await _bNetClient.Do<List<BNetLib.Models.Summary>>(new SummaryCommand());
                if (latest?.Seqn < seqn)
                {
                    latest = Manifest<BNetLib.Models.Summary[]>.Create(seqn, "summary", summary.ToArray());
                    try
                    {
                        latest.Raw = raw;
                        _dbContext.Summary.Add(latest);
                        _dbContext.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogCritical($"Failed to write summary: {ex}");
                    }

                    var latestItems = latest?.Content.ToList() ?? new List<BNetLib.Models.Summary>();
                    foreach (var item in summary)
                    {
                        try
                        {
                            await AddItemToData(item, latest.Seqn, _dbContext, cancellationToken);
                            _dbContext.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogCritical($"Failed to write {item.Product}-{item.Flags}-{item.Seqn}: {ex}");
                        }
                    }
                }

                TimeSpan ts = stopWatch.Elapsed;
                string elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
                _logger.LogDebug($"Version Tracking took {elapsedTime}");


                // Check every 5 seconds, at some point this might need to be proxied again, but until this i realllllllllllllllllllllllllly don't care
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }

        private async Task AddItemToData(BNetLib.Models.Summary msg, int parentSeqn, DBContext db, CancellationToken cancellationToken)
        {
            var code = msg.Product;

            switch (msg.Flags)
            {
                case "versions" or "version":
                    {
                        var exist = await db.Versions.AsNoTracking().OrderByDescending(x => x.Seqn).FirstOrDefaultAsync(x => x.Code == code && x.Seqn == msg.Seqn, cancellationToken: cancellationToken);

                        if (exist != null)
                        {
                            _logger.LogDebug($"Skipping {code}:{msg.Seqn}:{msg.Flags}");
                            return;
                        }
                        break;
                    }
                case "cdn" or "cdns":
                    {
                        var exist = await db.CDN.AsNoTracking().OrderByDescending(x => x.Seqn).FirstOrDefaultAsync(x => x.Code == code && x.Seqn == msg.Seqn, cancellationToken: cancellationToken);

                        if (exist != null)
                        {
                            _logger.LogDebug($"Skipping {code}:{msg.Seqn}:{msg.Flags}");
                            return;
                        }
                        break;
                    }
                case "bgdl":
                    {
                        var exist = await db.BGDL.AsNoTracking().OrderByDescending(x => x.Seqn).FirstOrDefaultAsync(x => x.Code == code && x.Seqn == msg.Seqn, cancellationToken: cancellationToken);

                        if (exist != null)
                        {
                            _logger.LogDebug($"Skipping {code}:{msg.Seqn}:{msg.Flags}");
                            return;
                        }
                        break;
                    }
                default:
                    _logger.LogCritical($"Unknown flag for {code}:{msg.Seqn}:{msg.Flags}");
                    return;
            }

            _logger.LogInformation($"We didn't skip: {code}-{msg.Seqn}-{msg.Flags}");

            // Update the config for only games we detect changes for
            await CheckifEncrypted(msg, db, _logger, cancellationToken);

            var (data, seqn, raw) = await GetMetaData(msg);
            if (seqn == -1) return;

            switch (data)
            {
                case List<BNetLib.Models.Versions> ver:
                    {
                        if (code == "catalogs")
                        {
                            await _channelWriter.WriteAsync(new ConfigUpdate()
                            {
                                Code = "catalogs",
                                Hash = ver.Last().Buildconfig,
                            }, cancellationToken);
                        } 
                        else
                        {
                            await _channelWriter.WriteAsync(new ConfigUpdate()
                            {
                                Code = code,
                                Hash = ver.Last().Productconfig,
                            }, cancellationToken);
                        }

                        var f = Manifest<BNetLib.Models.Versions[]>.Create(seqn, code, ver.ToArray());
                        f.Raw = raw;
                        f.Parent = parentSeqn;
                        db.Versions.Add(f);
                        break;
                    }
                case List<BNetLib.Models.CDN> cdn:
                    {
                        var f = Manifest<BNetLib.Models.CDN[]>.Create(seqn, code, cdn.ToArray());
                        f.Raw = raw;
                        f.Parent = parentSeqn;
                        db.CDN.Add(f);
                        break;
                    }
                case List<BNetLib.Models.BGDL> bgdl:
                    {
                        var f = Manifest<BNetLib.Models.BGDL[]>.Create(seqn, code, bgdl.ToArray());
                        f.Raw = raw;
                        f.Parent = parentSeqn;
                        db.BGDL.Add(f);
                        break;
                    }
                default:
                    _logger.LogCritical($"Unhandled type {data.GetType()}");
                    break;
            }
        }

        private async Task<(IList data, int seqn, string raw)> GetMetaData(BNetLib.Models.Summary msg)
        {
            (IList data, int seqn, string raw) res = msg.Flags switch
            {
                "version" or "versions" => await _bNetClient.Do<List<BNetLib.Models.Versions>>(new VersionCommand(msg.Product)),
                "cdn" or "cdns" => await _bNetClient.Do<List<BNetLib.Models.CDN>>(new CDNCommand(msg.Product)),
                "bgdl" => await _bNetClient.Do<List<BNetLib.Models.BGDL>>(new BGDLCommand(msg.Product)),
                _ => (null, -1, string.Empty)
            };

            if (res.seqn == -1) return (res.data, res.seqn, res.raw);

            if (res.seqn != msg.Seqn)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                return await GetMetaData(msg);
            }

            return (res.data, res.seqn, res.raw);
        }

        private async Task CheckifEncrypted(BNetLib.Models.Summary msg, DBContext dbContext, ILogger<Summary> _logger, CancellationToken cancellationToken)
        {
            if (msg.Flags == "cdn" || msg.Flags == "bgdl") return;
            var versionConfig = await dbContext.Versions.AsNoTracking().OrderByDescending(x => x.Seqn).FirstOrDefaultAsync(x => x.Code == msg.Product, cancellationToken: cancellationToken);

            var config = versionConfig.Content.FirstOrDefault(x => x.Region == "us");
            if (msg.Product == "catalogs") config = versionConfig.Content.Last();
            if (msg.Product == "bts") config = versionConfig.Content.FirstOrDefault(x => x.Region == "launcher");
            if (config == null) return;
            var currentGameConfig = await dbContext.GameConfigs.FirstOrDefaultAsync(x => x.Code == msg.Product, cancellationToken: cancellationToken);

            string file = string.Empty;
            try
            {
                file = await _productConfig.GetRaw(config.Productconfig);
            }
            catch
            {
                // Add missing game even if it's 404, this is for a later feature
                _logger.LogCritical($"{msg.Product} product config doesn't exist");
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
                }
                return;
            }

            _logger.LogDebug($"checking {msg.Product} if encrypted or not ");

            if (file.Contains("\"decryption_key_name\""))
            {
                dynamic f = JObject.Parse(file);

                if (currentGameConfig == null)
                {
                    await dbContext.GameConfigs.AddAsync(new GameConfig()
                    {
                        Code = msg.Product,
                        Config = new ConfigItems()
                        {
                            Encrypted = true,
                            EncryptedKey = f.all.config.decryption_key_name,
                        }
                    }, cancellationToken);
                }
                else
                {
                    currentGameConfig.Config.Encrypted = true;
                    currentGameConfig.Config.EncryptedKey = f.all.config.decryption_key_name;
                }
            }
            else
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
                }
            }
        }
    }
}