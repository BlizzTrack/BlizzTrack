using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Models;
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
        private readonly Twitter _twitter;

        public Notify(ILogger<Notify> logger, IRedisDatabase redisDatabase, Twitter twitter)
        {
            _logger = logger;
            _redisDatabase = redisDatabase;
            _twitter = twitter;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _redisDatabase.SubscribeAsync<Notification>("event_notifications", HandleNotifyEvent);
        }

        private async Task HandleNotifyEvent(Notification arg)
        {
            switch (arg.NotificationType)
            {
                case NotificationType.Versions:
                    await _twitter.Publish(arg.Payload);
                    break;
                case NotificationType.PatchNotes:
                    await _twitter.PublishPatchNotes(arg.Payload);
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"{arg.NotificationType} doesn't exist");
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _redisDatabase.UnsubscribeAllAsync();
        }
    }
}