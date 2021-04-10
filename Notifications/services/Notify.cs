using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Core.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Notifications.services
{
    public class Notify : IHostedService
    {
        private readonly ILogger<Notify> _logger;
        private readonly IRedisDatabase _redisDatabase;
        private readonly ChannelWriter<Notification> _channelWriter;

        public Notify(ILogger<Notify> logger, IRedisDatabase redisDatabase, ChannelWriter<Notification> channelWriter)
        {
            _logger = logger;
            _redisDatabase = redisDatabase;
            _channelWriter = channelWriter;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await _channelWriter.WriteAsync(new Notification()
                    {
                        NotificationType = NotificationType.Ping,
                        Payload = new Dictionary<string, object>
                        {
                            {"sent_at", DateTime.Now.Ticks}
                        }
                    }, cancellationToken);
                    
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                }
            }, cancellationToken);
            
            await _redisDatabase.SubscribeAsync<Notification>("event_notifications", HandleNotifyEvent);
        }

        private async Task HandleNotifyEvent(Notification arg)
        {
            _channelWriter.TryWrite(arg);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _redisDatabase.UnsubscribeAllAsync();
        }
    }
}