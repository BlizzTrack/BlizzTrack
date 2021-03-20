using BNetLib.Http;
using BNetLib.Networking;
using Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
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
        private readonly IServiceProvider _serviceProvider;

        public SummaryHosted(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(() =>
            {
                var c = ActivatorUtilities.CreateInstance<Summary>(_serviceProvider);
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
        private readonly ILogger<Summary> _logger;
        private readonly IServiceScopeFactory _serviceScope;
        private readonly ConcurrentQueue<ConfigUpdate> _channelWriter;

        public Summary(BNetClient bNetClient, ILogger<Summary> logger, IServiceScopeFactory scopeFactory, ConcurrentQueue<ConfigUpdate> channelWriter)
        {
            _bNetClient = bNetClient;
            _logger = logger;
            _serviceScope = scopeFactory;
            _channelWriter = channelWriter;
        }

        public async void Run(CancellationToken cancellationToken)
        {
            var firstRun = true;
            while (!cancellationToken.IsCancellationRequested)
            {
                var stopWatch = Stopwatch.StartNew();

                using var sc = _serviceScope.CreateScope();
                var summary = sc.ServiceProvider.GetRequiredService<Core.Services.ISummary>();
                var dbContext = sc.ServiceProvider.GetRequiredService<DBContext>();

                var latest = await summary.Latest();
                if (firstRun)
                {
                    foreach (var item in latest.Content)
                    {
                        await AddItemToData(item, latest.Seqn, dbContext, cancellationToken);
                    }

                    await dbContext.SaveChangesAsync(cancellationToken);

                    firstRun = false;
                }

                var res = await _bNetClient.Summary();
                if (latest?.Seqn < res.Seqn)
                {
                    latest = Manifest<BNetLib.Models.Summary[]>.Create(res.Seqn, "summary", res.Payload.ToArray());
                    try
                    {
                        latest.Raw = res.Raw;
                        await dbContext.Summary.AddAsync(latest, cancellationToken);
                        await dbContext.SaveChangesAsync(cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogCritical($"Failed to write summary: {ex}");
                    }

                    foreach (var item in res.Payload)
                    {
                        try
                        {
                            if (latest is not null)
                                await AddItemToData(item, latest.Seqn, dbContext, cancellationToken);
                            await dbContext.SaveChangesAsync(cancellationToken);
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

            object data;
            int seqn;
            string raw;

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

                        var (value, s, r) = await GetMetaData<BNetLib.Models.Versions>(msg);

                        data = value;
                        seqn = s;
                        raw = r;

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

                        var (value, s, r) = await GetMetaData<BNetLib.Models.CDN>(msg);

                        data = value;
                        seqn = s;
                        raw = r;
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


                        var (value, s, r) = await GetMetaData<BNetLib.Models.BGDL>(msg);

                        data = value;
                        seqn = s;
                        raw = r;
                        break;
                    }
                default:
                    _logger.LogCritical($"Unknown flag for {code}:{msg.Seqn}:{msg.Flags}");
                    return;
            }

            _logger.LogInformation($"We didn't skip: {code}-{msg.Seqn}-{msg.Flags}");

            // Update the config for only games we detect changes for

            if (seqn == -1) return;

            switch (data)
            {
                case List<BNetLib.Models.Versions> ver:
                    {
                        if(ver.Count > 0) {
                            if (msg.Product == "catalogs")
                            {
                                _channelWriter.Enqueue(new ConfigUpdate()
                                {
                                    Code = msg.Product,
                                    Hash = ver.Last().Buildconfig,
                                });
                            }
                            else
                            {
                                _channelWriter.Enqueue(new ConfigUpdate()
                                {
                                    Code = msg.Product,
                                    Hash = ver.Last().Productconfig,
                                });
                            }

                            var config = ver.FirstOrDefault(x => x.Region == "us");
                            if (msg.Product == "catalogs") config = ver.Last();
                            if (msg.Product == "bts") config = ver.FirstOrDefault(x => x.Region == "launcher");

                            if (config is not null)
                                await CheckifEncrypted(msg, config.Productconfig, db, _logger, cancellationToken);
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

        private async Task<(object Value, int Seqn, string Raw)> GetMetaData<T>(BNetLib.Models.Summary msg) where T : class, new()
        {
            IList<T> data;
            int seqn;
            string raw;

            switch (typeof(T))
            {
                case { } versionType when versionType == typeof(BNetLib.Models.Versions):
                    {
                        var payload = await _bNetClient.Versions(msg.Product);
                        data = (IList<T>)payload.Payload;
                        seqn = payload.Seqn;
                        raw = payload.Raw;
                    }
                    break;

                case { } bgdlType when bgdlType == typeof(BNetLib.Models.BGDL):
                    {
                        var payload = await _bNetClient.BGDL(msg.Product);
                        data = (IList<T>)payload.Payload;
                        seqn = payload.Seqn;
                        raw = payload.Raw;
                    }
                    break;

                case { } cdnType when cdnType == typeof(BNetLib.Models.CDN):
                    {
                        var payload = await _bNetClient.CDN(msg.Product);
                        data = (IList<T>)payload.Payload;
                        seqn = payload.Seqn;
                        raw = payload.Raw;
                    }
                    break;

                default:
                    return (null, -1, null);
            }

            if (seqn != msg.Seqn)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                return await GetMetaData<T>(msg);
            }

            return (data, seqn, raw);
        }

        private async Task CheckifEncrypted(BNetLib.Models.Summary msg, string productConfig, DBContext dbContext, ILogger<Summary> logger, CancellationToken cancellationToken)
        {
            var (product, _, flags) = msg;
            if (flags == "cdn" || flags == "bgdl") return;

          
            var currentGameConfig = await dbContext.GameConfigs.FirstOrDefaultAsync(x => x.Code == product, cancellationToken: cancellationToken);

            string file;

            if(string.IsNullOrWhiteSpace(productConfig))
            {
                return;
            }

            try
            {
                file = await ProductConfig.GetRaw(productConfig);
            }
            catch
            {
                // Add missing game even if it's 404, this is for a later feature
                logger.LogCritical($"{product} product config doesn't exist");
                if (currentGameConfig == null)
                {
                    await dbContext.GameConfigs.AddAsync(new GameConfig()
                    {
                        Code = product,
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

            logger.LogDebug($"checking {product} if encrypted or not ");

            if (file.Contains("\"decryption_key_name\""))
            {
                dynamic f = JObject.Parse(file);

                if (currentGameConfig == null)
                {
                    await dbContext.GameConfigs.AddAsync(new GameConfig()
                    {
                        Code = product,
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
                        Code = product,
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