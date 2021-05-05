using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Core.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Notifications.services
{
    public class Sender : IHostedService
    {
        private readonly Twitter _twitter;
        private readonly ILogger<Sender> _logger;
        private readonly ChannelReader<Notification> _reader;
        
        public Sender(Twitter twitter, ILogger<Sender> logger, ChannelReader<Notification> reader)
        {
            _twitter = twitter;
            _logger = logger;
            _reader = reader;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogDebug("Waiting for channel events");
                var arg = await _reader.ReadAsync(cancellationToken);
                if (arg.NotificationType == NotificationType.Ping)
                {
                    _logger.LogDebug($"Got ping event: {arg.Payload["sent_at"]}");
                    continue;
                }
                
                _logger.LogInformation($"Got channel event: {arg.NotificationType}");
                
                switch (arg.NotificationType)
                {
                    case NotificationType.Versions:
                        // For sake of twitter lets not send versions, this is causing to much tweet spam
                        // await _twitter.Publish(arg.Payload);
                        break;
                    case NotificationType.PatchNotes:
                        await _twitter.PublishPatchNotes(arg.Payload);
                        break;
                    case NotificationType.Ping:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"{arg.NotificationType} doesn't exist");
                }
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
        }
    }
}