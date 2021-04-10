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
            Task.Run(async () =>
            {
                while (await _reader.WaitToReadAsync(cancellationToken))
                {
                    if (_reader.TryRead(out var arg))
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
                }
            }, cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
        }
    }
}