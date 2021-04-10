using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Services;
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

        public Twitter(TwitterClient twitterClient, ILogger<Twitter> logger, IVersions versions, IBGDL bgdl, IGameChildren gameChildren, IGameParents gameParents)
        {
            _twitterClient = twitterClient;
            _logger = logger;
            _versions = versions;
            _bgdl = bgdl;
            _gameChildren = gameChildren;
            _gameParents = gameParents;
        }

        public async Task Publish(Dictionary<string, object> data)
        {
            var code = (string) data["code"];
            var flag = (string) data["flags"];

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