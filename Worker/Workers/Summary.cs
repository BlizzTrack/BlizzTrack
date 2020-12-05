using BNetLib.Networking;
using BNetLib.Networking.Commands;
using Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
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
        private readonly ChannelWriter<BNetLib.Models.Summary> _channelWriter;
        private readonly ChannelWriter<object> _databaseWriter;
        private readonly IServiceScopeFactory _serviceScope;

        public Summary(BNetClient bNetClient, ILogger<Summary> logger, ChannelWriter<BNetLib.Models.Summary> channelWriter, ChannelWriter<object> databaseWriter, IServiceScopeFactory scopeFactory)
        {
            _bNetClient = bNetClient;
            _logger = logger;
            _channelWriter = channelWriter;
            _databaseWriter = databaseWriter;
            _serviceScope = scopeFactory;
        }

        public async void Run(CancellationToken cancellationToken)
        {
            using var sc = _serviceScope.CreateScope();
            var _summary = sc.ServiceProvider.GetRequiredService<Core.Services.ISummary>();

            while (!cancellationToken.IsCancellationRequested)
            {
                (var summary, int seqn) = await _bNetClient.Do<List<BNetLib.Models.Summary>>(new SummaryCommand());
                var latest = await _summary.Latest();

                if (latest == null || latest.Seqn != seqn)
                {
                    // Do this first so it's always done first when in the channel
                    await _databaseWriter.WriteAsync(Manifest<BNetLib.Models.Summary[]>.Create(seqn, "summary", summary.ToArray()), cancellationToken);

                    var latestItems = latest?.Content.ToList() ?? new List<BNetLib.Models.Summary>();
                    foreach (var item in summary)
                    {
                        var exist = latestItems.FirstOrDefault(x => x.Product == item.Product && x.Flags == item.Flags && x.Seqn == item.Seqn);
                        if (exist == null)
                        {
                            await _channelWriter.WriteAsync(item, cancellationToken);
                        }
                    }
                }
                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
            }
        }
    }
}
