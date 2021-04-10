using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Models;
using Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tweetinvi;
using Tweetinvi.Models;

namespace Notifications.services
{
    public class Twitter : IPublishService
    {
        private readonly TwitterClient _twitterClient;
        private readonly ILogger<Twitter> _logger;
        private readonly IVersions _versions;
        private readonly IBGDL _bgdl;
        private readonly IGameChildren _gameChildren;
        private readonly IGameParents _gameParents;
        private readonly DBContext _dbContext;

        public Twitter(TwitterClient twitterClient, ILogger<Twitter> logger, IVersions versions, IBGDL bgdl, IGameChildren gameChildren, IGameParents gameParents, DBContext dbContext)
        {
            _twitterClient = twitterClient;
            _logger = logger;
            _versions = versions;
            _bgdl = bgdl;
            _gameChildren = gameChildren;
            _gameParents = gameParents;
            _dbContext = dbContext;
        }

        public async Task Publish(Dictionary<string, object> data)
        {
            var code = (string) data["code"];
            var flag = (string) data["flags"];
            var seqn = (long) data["seqn"];

            if (await _dbContext.NotificationHistories.Where(x =>
                x.Seqn == seqn.ToString() && x.NotificationType == NotificationType.Versions && x.Code == code &&
                x.File == flag).FirstOrDefaultAsync() != null)
            {
                return;
            }

            await _dbContext.NotificationHistories.AddAsync(new NotificationHistory()
            {
                NotificationType = NotificationType.Versions,
                Code = code,
                Sent = DateTime.UtcNow,
                File = flag,
                Seqn = $"{seqn}",
            });
            await _dbContext.SaveChangesAsync();
            
            var child = await _gameChildren.Get(code);
            
            var previousSeqn = flag switch
            {
                "versions" or "version" => (await _versions.Previous(code))?.Seqn,
                "bgdl" => (await _bgdl.Previous(code))?.Seqn,
                _ => -1
            };

            try
            {
                ITweet tweet;
                if (previousSeqn != null)
                {
                    if (previousSeqn == -1) return;

                    tweet = await _twitterClient.Tweets.PublishTweetAsync(
                        $"{child.Name} updated from seqn {previousSeqn} to seqn {data["seqn"]}\nhttps://blizztrack.com/v/{child.Slug}/{flag.ToLower()}?latest-seqn={data["seqn"]}");
                }
                else
                {
                    tweet = await _twitterClient.Tweets.PublishTweetAsync(
                        $"{child.Name} was just updated to seqn {data["seqn"]}\nhttps://blizztrack.com/v/{child.Slug}/{flag.ToLower()}?latest-seqn={data["seqn"]}");
                }

                _logger.LogInformation($"Sent new tweet for {code}: {tweet.IdStr}");
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Failed to send to twitter: {ex}");
            }
        }

        public async Task PublishPatchNotes(Dictionary<string, object> data)
        {
            var code = (string) data["code"];
            var flag = (string) data["flags"];
            var time = (long) data["index_time"];
            
            if (await _dbContext.NotificationHistories.Where(x =>
                x.Seqn == time.ToString() && x.NotificationType == NotificationType.PatchNotes && x.Code == code &&
                x.File == flag).FirstOrDefaultAsync() != null)
            {
                return;
            }

            await _dbContext.NotificationHistories.AddAsync(new NotificationHistory()
            {
                NotificationType = NotificationType.PatchNotes,
                Code = code,
                Sent = DateTime.UtcNow,
                File = flag,
                Seqn = $"{time}",
            });
            await _dbContext.SaveChangesAsync();

            var child = await _gameChildren.Get(code);

            var parent = await _gameParents.Get(code);

            try
            {
                var tweet = await _twitterClient.Tweets.PublishTweetAsync(
                    $"{child.Name} has new patch notes\nhttps://blizztrack.com/patch-notes/{parent.Slug}/{flag.ToLower()}?build_time={time}");

                _logger.LogInformation($"Sent new tweet for {code}: {tweet.IdStr}");
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Failed to send to twitter: {ex}");
            }
        }
    }
}