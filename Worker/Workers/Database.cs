using BNetLib.Models;
using Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
        private readonly DBContext _dbContext;
        public Database(ChannelReader<object> channelReader, ILogger<Database> logger, DBContext dbContext)
        {
            _channelReader = channelReader;
            _logger = logger;
            _dbContext = dbContext;
        }

        internal async void Run(CancellationToken cancellationToken)
        {
            while(await _channelReader.WaitToReadAsync(cancellationToken))
            {
                if(_channelReader.TryRead(out var item))
                {
                    await _dbContext.AddAsync(item, cancellationToken);
                    await _dbContext.SaveChangesAsync(cancellationToken);

                    switch(item)
                    {
                        case Manifest<Versions[]> ver:
                            _logger.LogInformation($"Update {ver.Code} versions to {ver.Seqn}");
                            break;
                        case Manifest<BGDL[]> ver:
                            _logger.LogInformation($"Update {ver.Code} BDGL to {ver.Seqn}");
                            break;
                        case Manifest<CDN[]> ver:
                            _logger.LogInformation($"Update {ver.Code} CDNs to {ver.Seqn}");
                            break;
                        case Manifest<Summary[]> ver:
                            _logger.LogInformation($"Update {ver.Code} summary to {ver.Seqn}");
                            break;
                    }
                }
            }
        }
    }
}
