using BNetLib.Networking;
using BNetLib.Networking.Commands;
using Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
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
        private readonly ILogger<Summary> _logger;
        private readonly IServiceScopeFactory _serviceScope;

        public Summary(BNetClient bNetClient, ILogger<Summary> logger, IServiceScopeFactory scopeFactory)
        {
            _bNetClient = bNetClient;
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
                var _dbContext = sc.ServiceProvider.GetRequiredService<Core.Models.DBContext>();

                if (firstRun)
                {
                    var f = await _summary.Latest();
                    foreach (var item in f.Content)
                    {
                        await AddItemToData(item, _dbContext, cancellationToken);
                    }

                    await _dbContext.SaveChangesAsync(cancellationToken);

                    firstRun = false;
                }

                (var summary, int seqn) = await _bNetClient.Do<List<BNetLib.Models.Summary>>(new SummaryCommand());
                var latest = await _summary.Latest();

                if (latest == null || latest.Seqn != seqn)
                {
                    await _dbContext.AddAsync(Manifest<BNetLib.Models.Summary[]>.Create(seqn, "summary", summary.ToArray()), cancellationToken);

                    var latestItems = latest?.Content.ToList() ?? new List<BNetLib.Models.Summary>();
                    foreach (var item in summary)
                    {
                        var exist = latestItems.FirstOrDefault(x => x.Product == item.Product && x.Flags == item.Flags && x.Seqn == item.Seqn);
                        if (exist == null)
                        {
                            await AddItemToData(item, _dbContext, cancellationToken);
                        }
                    }

                    await _dbContext.SaveChangesAsync(cancellationToken);
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
                        var exist = await db.Versions.FirstOrDefaultAsync(x => x.Code == code && x.Seqn == msg.Seqn, cancellationToken: cancellationToken);

                        if (exist != null)
                        {
                            _logger.LogInformation($"Skipping {code}:{msg.Seqn}:{msg.Flags}");
                            return;
                        }
                        break;
                    }
                case "cdn" or "cdns":
                    {
                        var exist = await db.CDN.FirstOrDefaultAsync(x => x.Code == code && x.Seqn == msg.Seqn, cancellationToken: cancellationToken);

                        if (exist != null)
                        {
                            _logger.LogInformation($"Skipping {code}:{msg.Seqn}:{msg.Flags}");
                            return;
                        }
                        break;
                    }
                case "bgdl":
                    {
                        var exist = await db.BGDL.FirstOrDefaultAsync(x => x.Code == code && x.Seqn == msg.Seqn, cancellationToken: cancellationToken);

                        if (exist != null)
                        {
                            _logger.LogInformation($"Skipping {code}:{msg.Seqn}:{msg.Flags}");
                            return;
                        }
                        break;
                    }
            }

            (IList data, int seqn) res = msg.Flags switch
            {
                "version" or "versions" => await _bNetClient.Do<List<BNetLib.Models.Versions>>(new VersionCommand(code)),
                "cdn" or "cdns" => await _bNetClient.Do<List<BNetLib.Models.CDN>>(new CDNCommand(code)),
                "bgdl" => await _bNetClient.Do<List<BNetLib.Models.BGDL>>(new BGDLCommand(code)),
                _ => (null, -1)
            };

            switch (res.data)
            {
                case List<BNetLib.Models.Versions> ver:
                    {
                        var f = Manifest<BNetLib.Models.Versions[]>.Create(res.seqn, code, ver.ToArray());
                        await db.AddAsync(f, cancellationToken);
                        break;
                    }
                case List<BNetLib.Models.CDN> cdn:
                    {
                        var f = Manifest<BNetLib.Models.CDN[]>.Create(res.seqn, code, cdn.ToArray());
                        await db.AddAsync(f, cancellationToken);
                        break;
                    }
                case List<BNetLib.Models.BGDL> bgdl:
                    {
                        var f = Manifest<BNetLib.Models.BGDL[]>.Create(res.seqn, code, bgdl.ToArray());
                        await db.AddAsync(f, cancellationToken);
                        break;
                    }
                default:
                    _logger.LogCritical("Unhandled type");
                    break;
            }
        }
    }
}
