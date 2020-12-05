using BNetLib.Models;
using Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Worker.Workers
{
    class DatabaseHosted : IHostedService
    {
        private readonly IServiceProvider serviceProvider;

        public DatabaseHosted(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(() => {
                var c = ActivatorUtilities.CreateInstance<Database>(serviceProvider);
                c.Run(cancellationToken);
            }, cancellationToken);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    class Database
    {
        private readonly ChannelReader<object> _channelReader;
        private readonly ILogger<Database> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public Database(ChannelReader<object> channelReader, ILogger<Database> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _channelReader = channelReader;
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        internal async void Run(CancellationToken cancellationToken)
        {
            while(await _channelReader.WaitToReadAsync(cancellationToken))
            {
                using var sc = _serviceScopeFactory.CreateScope();
                var _dbContext = sc.ServiceProvider.GetRequiredService<DBContext>();

                if (_channelReader.TryRead(out var item))
                {
                    try
                    {
                        await _dbContext.AddAsync(item, cancellationToken);
                        await _dbContext.SaveChangesAsync(cancellationToken);
                    }catch(Exception ex)
                    {
                        _logger.LogCritical(ex.ToString());
                        Debugger.Break();
                    }

                    switch(item)
                    {
                        case Manifest<BNetLib.Models.Versions[]> ver:
                            _logger.LogInformation($"Update {ver.Code} versions to {ver.Seqn}");
                            break;
                        case Manifest<BGDL[]> ver:
                            _logger.LogInformation($"Update {ver.Code} BDGL to {ver.Seqn}");
                            break;
                        case Manifest<CDN[]> ver:
                            _logger.LogInformation($"Update {ver.Code} CDNs to {ver.Seqn}");
                            break;
                        case Manifest<BNetLib.Models.Summary[]> ver:
                            _logger.LogInformation($"Update {ver.Code} summary to {ver.Seqn}");
                            break;
                    }
                }
            }
        }
    }
}
