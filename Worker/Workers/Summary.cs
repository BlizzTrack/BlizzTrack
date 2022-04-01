using BNetLib.Http;
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
using BNetLib.Ribbit;
using BNetLib.Ribbit.Models;
using Core.Extensions;
using StackExchange.Redis.Extensions.Core.Abstractions;
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
        private readonly IRedisDatabase _redisDatabase;

        public Summary(BNetClient bNetClient, ILogger<Summary> logger, IServiceScopeFactory scopeFactory,
            ConcurrentQueue<ConfigUpdate> channelWriter, IRedisDatabase redisDatabase)
        {
            _bNetClient = bNetClient;
            _logger = logger;
            _serviceScope = scopeFactory;
            _channelWriter = channelWriter;
            _redisDatabase = redisDatabase;
        }

        public async void Run(CancellationToken cancellationToken)
        {
            using var sc = _serviceScope.CreateScope();
            var summary = sc.ServiceProvider.GetRequiredService<Core.Services.ISummary>();
            var parents = sc.ServiceProvider.GetRequiredService<Core.Services.IGameParents>();
            var dbContext = sc.ServiceProvider.GetRequiredService<DBContext>();

            var firstRun = true;
            while (!cancellationToken.IsCancellationRequested)
            {
                var stopWatch = Stopwatch.StartNew();

                var updated = new List<(string code, string file, int seqn)>();
                
                var latest = await summary.Latest();
                if (firstRun)
                {
                    int id = 0;
                    foreach (var item in latest.Content)
                    {
                        id++;
                        _logger.LogDebug($"{id} {latest.Content.Length} {item.Product} {item.Seqn} {item.Flags}");
                        
                        var parent = await parents.Get(item.Product) ?? await parents.Get("unknown");

                        var gameChildData =
                            await dbContext.GameChildren.FirstOrDefaultAsync(x => x.Code == item.Product,
                                CancellationToken.None);
                        
                        /*
                        if (await AddItemToData(item, latest.Seqn, dbContext, cancellationToken, gameChildData))
                        {
                            updated.Add((item.Product, item.Flags, item.Seqn));
                        }
                        */
                        
                        if (gameChildData == null)
                        {

                            var config = await dbContext.GameConfigs.FirstOrDefaultAsync(
                                x => x.Code == item.Product, CancellationToken.None);
                            var name = string.IsNullOrEmpty(config?.Name)
                                ? BNetLib.Helpers.GameName.Get(item.Product)
                                : config?.Name;

                            var slugInUse = await dbContext.GameChildren.Where(x => x.Slug == name.Slugify())
                                .FirstOrDefaultAsync(CancellationToken.None);

                            gameChildData = new GameChildren
                            {
                                Code = item.Product,
                                ParentCode = parent.Code,
                                Name = name,
                                GameConfig = config,
                                Slug = slugInUse == null ? name.Slugify() : item.Product
                            };
                            
                            await dbContext.GameChildren.AddAsync(gameChildData, CancellationToken.None);
                        }
                        
                        if (await AddItemToData(item, latest.Seqn, dbContext, CancellationToken.None, gameChildData))
                        {
                            _logger.LogDebug("Trying to update game");
                            if(!updated.Exists(x => x.code == item.Product && x.seqn == item.Seqn && x.file == item.Flags))
                                updated.Add((item.Product, item.Flags, item.Seqn));
                        }
                    }
                    
                    if(dbContext.ChangeTracker.HasChanges())
                        await dbContext.SaveChangesAsync(CancellationToken.None);

                    foreach (var (code, file, seqn) in updated)
                    {
                        await SendTwitterAlert(file, code, seqn);
                    }
                    
                    firstRun = false;
                }

                var res = await _bNetClient.Summary();
                if (latest?.Seqn < res.Seqn)
                {
                    latest = Manifest<BNetLib.Ribbit.Models.Summary[]>.Create(res.Seqn, "summary", res.Payload.ToArray());
                    try
                    {
                        latest.Raw = res.Raw;
                        await dbContext.Summary.AddAsync(latest, CancellationToken.None);
                        await dbContext.SaveChangesAsync(CancellationToken.None);
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
                            {
                                var parent = await parents.Get(item.Product) ?? await parents.Get("unknown");

                                var gameChildData =
                                    await dbContext.GameChildren.FirstOrDefaultAsync(x => x.Code == item.Product, CancellationToken.None);
                                
                                if (gameChildData == null)
                                {
                                    var config = await dbContext.GameConfigs.FirstOrDefaultAsync(
                                        x => x.Code == item.Product, CancellationToken.None);
                                    var name = string.IsNullOrEmpty(config?.Name) ? BNetLib.Helpers.GameName.Get(item.Product) : config?.Name;

                                    var slugInUse = await dbContext.GameChildren.Where(x => x.Slug == name.Slugify())
                                        .FirstOrDefaultAsync(CancellationToken.None);

                                    gameChildData = new GameChildren
                                    {
                                        Code = item.Product,
                                        ParentCode = parent.Code,
                                        Name = name,
                                        GameConfig = config,
                                        Slug = slugInUse == null ? name.Slugify() : item.Product
                                    };

                                    await dbContext.GameChildren.AddAsync(gameChildData, CancellationToken.None);
                                }
                                
                                if (await AddItemToData(item, latest.Seqn, dbContext, CancellationToken.None, gameChildData))
                                {
                                    if(!updated.Exists(x => x.code == item.Product && x.seqn == item.Seqn && x.file == item.Flags))
                                        updated.Add((item.Product, item.Flags, item.Seqn));
                                }
                            }
                            
                            await dbContext.SaveChangesAsync(CancellationToken.None);
                            
                            foreach (var (code, file, seqn) in updated)
                            {
                                await SendTwitterAlert(file, code, seqn);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogCritical($"Failed to write {item.Product}-{item.Flags}-{item.Seqn}: {ex}");
                        }
                    }
                }

                var ts = stopWatch.Elapsed;
                var elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
                _logger.LogDebug($"Version Tracking took {elapsedTime}");

                // Check every 5 seconds, at some point this might need to be proxied again, but until this i realllllllllllllllllllllllllly don't care
                updated.Clear();
                
                await Task.Delay(TimeSpan.FromSeconds(5), CancellationToken.None);
            }
        }

        private async Task<bool> AddItemToData(BNetLib.Ribbit.Models.Summary msg, int parentSeqn, DBContext db, CancellationToken cancellationToken, GameChildren owner)
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
                            return false;
                        }

                        var (value, s, r) = await GetMetaData<Versions>(msg);

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
                            return false;
                        }

                        var (value, s, r) = await GetMetaData<CDN>(msg);

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
                            return false;
                        }

                        var (value, s, r) = await GetMetaData<BGDL>(msg);

                        data = value;
                        seqn = s;
                        raw = r;
                        break;
                    }
                default:
                    _logger.LogCritical($"Unknown flag for {code}:{msg.Seqn}:{msg.Flags}");
                    return false;
            }

            _logger.LogInformation($"We didn't skip: {code}-{msg.Seqn}-{msg.Flags}");

            // Update the config for only games we detect changes for

            if (seqn == -1) return false;

            switch (data)
            {
                case List<Versions> ver:
                    {
                        if(ver.Count > 0) {
                            if (msg.Product == "catalogs")
                            {
                                _channelWriter.Enqueue(new ConfigUpdate
                                {
                                    Code = msg.Product,
                                    Hash = ver.Last().Buildconfig,
                                });
                            }
                            else
                            {
                                // Skip bootstrapper for product config validation as it doesn't have one
                                if (msg.Product != "bts")
                                {
                                    _channelWriter.Enqueue(new ConfigUpdate
                                    {
                                        Code = msg.Product,
                                        Hash = ver.Last().Productconfig,
                                    });
                                }
                            }

                            var config = msg.Product switch
                            {
                                "catalogs" => ver.Last(),
                                "bts" => ver.FirstOrDefault(x => x.Region == "launcher"),
                                _ => ver.FirstOrDefault(x => x.Region == "us")
                            };
                            
                            if (config is not null)
                                await CheckIfEncrypted(msg, config.Productconfig, db, _logger, cancellationToken, owner);
                        }

                        var f = Manifest<Versions[]>.Create(seqn, code, ver.ToArray());
                        f.Raw = raw;
                        f.Parent = parentSeqn;
                        var cfg = await db.GameConfigs.FirstOrDefaultAsync(x => x.Code == f.Code, cancellationToken);
                        if(cfg != null)
                            f.ConfigId = cfg.Code;
                        
                        await db.Versions.AddAsync(f, cancellationToken);

                        break;
                    }
                case List<CDN> cdn:
                    {
                        var f = Manifest<CDN[]>.Create(seqn, code, cdn.ToArray());
                        f.Raw = raw;
                        f.Parent = parentSeqn;
                        
                        var cfg = await db.GameConfigs.FirstOrDefaultAsync(x => x.Code == f.Code, cancellationToken);
                        if(cfg != null)
                            f.ConfigId = cfg.Code;
                        
                        await db.CDN.AddAsync(f, cancellationToken);
                        break;
                    }
                case List<BGDL> bgdl:
                    {
                        var f = Manifest<BGDL[]>.Create(seqn, code, bgdl.ToArray());
                        f.Raw = raw;
                        f.Parent = parentSeqn;
                        
                        var cfg = await db.GameConfigs.FirstOrDefaultAsync(x => x.Code == f.Code, cancellationToken);
                        if(cfg != null)
                            f.ConfigId = cfg.Code;
                        
                        await db.BGDL.AddAsync(f, cancellationToken);
                        
                        break;
                    }
                default:
                    _logger.LogCritical($"Unhandled type {data.GetType()}");
                    break;
            }

            return true;
        }

        private async Task<(object Value, int Seqn, string Raw)> GetMetaData<T>(BNetLib.Ribbit.Models.Summary msg) where T : class, new()
        {
            if(msg.Product == "odinv7" && msg.Flags == "versions")
                Debugger.Break();
            
            while (true)
            {
                IList<T> data;
                int seqn;
                string raw;

                switch (typeof(T))
                {
                    case { } versionType when versionType == typeof(Versions):
                    {
                        var payload = await _bNetClient.Versions(msg.Product);
                        data = (IList<T>) payload.Payload;
                        seqn = payload.Seqn;
                        raw = payload.Raw;
                    }
                        break;

                    case { } bgdlType when bgdlType == typeof(BGDL):
                    {
                        var payload = await _bNetClient.Bgdl(msg.Product);
                        data = (IList<T>) payload.Payload;
                        seqn = payload.Seqn;
                        raw = payload.Raw;
                    }
                        break;

                    case { } cdnType when cdnType == typeof(CDN):
                    {
                        var payload = await _bNetClient.Cdn(msg.Product);
                        data = (IList<T>) payload.Payload;
                        seqn = payload.Seqn;
                        raw = payload.Raw;
                    }
                        break;

                    default:
                        return (null, -1, null);
                }

                if (seqn >= msg.Seqn) return (data, seqn, raw);
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        private static async Task CheckIfEncrypted(BNetLib.Ribbit.Models.Summary msg, string productConfig, DBContext dbContext, ILogger<Summary> logger, CancellationToken cancellationToken, GameChildren owner)
        {
            var (product, _, flags) = msg;
            if (flags == "cdn" || flags == "bgdl") return;
            
            var currentGameConfig = await dbContext.GameConfigs.FirstOrDefaultAsync(x => x.Code == product, cancellationToken);

            string file;

            if(string.IsNullOrWhiteSpace(productConfig))
            {
                return;
            }

            if (msg.Product == "bts")
            {
                file = string.Empty;
            }
            else
            {
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
                        await dbContext.GameConfigs.AddAsync(new GameConfig
                        {
                            Code = product,
                            Config = new ConfigItems(false, string.Empty),
                            Logos = new List<Icons>(),
                            Name =  owner.Name,
                            Owner = owner
                        }, cancellationToken);
                    }
                    else
                    {
                        currentGameConfig.Config.Encrypted = false;
                    }

                    return;
                }
            }

            logger.LogDebug($"checking {product} if encrypted or not ");

            if (file.Contains("\"decryption_key_name\""))
            {
                dynamic f = JObject.Parse(file);

                if (currentGameConfig == null)
                {
                    var x = f.all.config.decryption_key_name;
                    await dbContext.GameConfigs.AddAsync(new GameConfig
                    {
                        Code = product,
                        Config = new ConfigItems(true, x.ToString()),
                        Logos = new List<Icons>(),
                        Name =  owner.Name,
                        Owner = owner              
                    }, cancellationToken);
                }
                else
                {
                    currentGameConfig.Config.Encrypted = true;
                    currentGameConfig.Config.EncryptedKey = f.all.config.decryption_key_name.ToString();
                }
            }
            else
            {
                if (currentGameConfig == null)
                {
                    await dbContext.GameConfigs.AddAsync(new GameConfig
                    {
                        Code = product,
                        Config = new ConfigItems(false, string.Empty),
                        Logos = new List<Icons>(),
                        Name =  owner.Name,
                        Owner = owner,
                    }, cancellationToken);
                }
                else
                {
                    currentGameConfig.Config.Encrypted = false;
                }
            }
        }

        private async Task SendTwitterAlert(string file, string code, int seqn)
        {
            await _redisDatabase.PublishAsync("event_notifications", new Notification
            {
                NotificationType = NotificationType.Versions,
                Payload = new Dictionary<string, object>
                {
                    { "code", code },
                    { "seqn", seqn },
                    { "flags", file }
                }
            });
        }
    }
}