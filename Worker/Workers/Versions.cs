﻿using BNetLib.Models;
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
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Worker.Workers
{
    class VersionsHosted : IHostedService
    {
        private readonly IServiceProvider serviceProvider;

        public VersionsHosted(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(() =>
            {
                var c = ActivatorUtilities.CreateInstance<Versions>(serviceProvider);
                c.Run(cancellationToken);
            }, cancellationToken);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    class Versions
    {
        private readonly ILogger<Versions> _logger;
        private readonly BNetClient _bNetClient;
        private readonly ChannelReader<BNetLib.Models.Summary> _channelReader;
        private readonly ChannelWriter<object> _databaseWriter;
        private readonly IServiceScopeFactory _serviceScope;

        public Versions(BNetClient bNetClient, ILogger<Versions> logger, ChannelReader<BNetLib.Models.Summary> channelReader, ChannelWriter<object> databaseWriter, IServiceScopeFactory serviceScope)
        {
            _bNetClient = bNetClient;
            _logger = logger;
            _channelReader = channelReader;
            _databaseWriter = databaseWriter;
            _serviceScope = serviceScope;
        }

        public async void Run(CancellationToken cancellationToken)
        {
            while (await _channelReader.WaitToReadAsync(cancellationToken))
            {
                using var sc = _serviceScope.CreateScope();
                var db = sc.ServiceProvider.GetRequiredService<DBContext>();

                if (_channelReader.TryRead(out var msg))
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
                                    continue;
                                }
                                break;
                            }
                        case "cdn" or "cdns":
                            {
                                var exist = await db.CDN.FirstOrDefaultAsync(x => x.Code == code && x.Seqn == msg.Seqn, cancellationToken: cancellationToken);

                                if (exist != null)
                                {
                                    _logger.LogInformation($"Skipping {code}:{msg.Seqn}:{msg.Flags}");
                                    continue;
                                }
                                break;
                            }
                        case "bgdl":
                            {
                                var exist = await db.BGDL.FirstOrDefaultAsync(x => x.Code == code && x.Seqn == msg.Seqn, cancellationToken: cancellationToken);

                                if (exist != null)
                                {
                                    _logger.LogInformation($"Skipping {code}:{msg.Seqn}:{msg.Flags}");
                                    continue;
                                }
                                break;
                            }
                    }

                    (IList data, int seqn) res = msg.Flags switch
                    {
                        "version" or "versions" => await _bNetClient.Do<List<BNetLib.Models.Versions>>(new VersionCommand(code)),
                        "cdn" or "cdns" => await _bNetClient.Do<List<CDN>>(new CDNCommand(code)),
                        "bgdl" => await _bNetClient.Do<List<BGDL>>(new BGDLCommand(code)),
                        _ => (null, -1)
                    };

                    switch (res.data)
                    {
                        case List<BNetLib.Models.Versions> ver:
                            {
                                await _databaseWriter.WriteAsync(Manifest<BNetLib.Models.Versions[]>.Create(res.seqn, code, ver.ToArray()), cancellationToken);
                                break;
                            }
                        case List<CDN> cdn:
                            {
                                await _databaseWriter.WriteAsync(Manifest<CDN[]>.Create(res.seqn, code, cdn.ToArray()), cancellationToken);
                                break;
                            }
                        case List<BGDL> bgdl:
                            {
                                await _databaseWriter.WriteAsync(Manifest<BGDL[]>.Create(res.seqn, code, bgdl.ToArray()), cancellationToken);
                                break;
                            }
                        default:
                            _logger.LogCritical("Unhandled type");
                            break;
                    }
                }
            }
        }
    }
}
