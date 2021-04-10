using System;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis.Extensions.Core.Abstractions;
using Tweetinvi;

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