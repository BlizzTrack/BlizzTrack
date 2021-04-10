using Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using BlizzTrack.Services;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.FeatureManagement.Mvc;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace BlizzTrack.API
{
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    [FeatureGate("Debug_Mode")]
    [ApiController]
    public class DebugController : ControllerBase
    {
        private readonly ILogger<DebugController> _logger;
        private readonly IRedisDatabase _redisDatabase;
        private readonly DBContext _dbContext;
        private readonly IVersions _versions;
        private readonly IPatchNotes _patchNotes;

        public DebugController(ILogger<DebugController> logger, IRedisDatabase redisDatabase, DBContext dbContext, IVersions versions, IPatchNotes patchNotes)
        {
            _logger = logger;
            _redisDatabase = redisDatabase;
            _dbContext = dbContext;
            _versions = versions;
            _patchNotes = patchNotes;
        }

        #region /game/:code/versions

        [HttpGet("game/{code}/versions")]
        public async Task<IActionResult> Versions([FromServices] IVersions versionService, string code)
        {
            var his = await versionService.Take(code, 2);
            var latest = his.First();
            var previous = his.Last();

            return Ok(new
            {
                latest = new
                {
                    latest.Indexed,
                    latest.Seqn,
                    command = new BNetLib.Networking.Commands.VersionCommand(code).ToString(),
                    Content = latest.Content.Select(x => new
                    {
                        region = x.GetName(),
                        x.Versionsname,
                        x.Buildid,
                        x.Buildconfig,
                        x.Productconfig,
                        x.Cdnconfig
                    }).ToList(),
                },
                previous = new
                {
                    previous.Indexed,
                    previous.Seqn,
                    command = new BNetLib.Networking.Commands.VersionCommand(code).ToString(),
                    Content = previous.Content.Select(x => new
                    {
                        region = x.GetName(),
                        x.Versionsname,
                        x.Buildid,
                        x.Buildconfig,
                        x.Productconfig,
                        x.Cdnconfig
                    }).ToList(),
                }
            });
        }

        #endregion /game/:code/versions

        #region /starts-with/:code

        [HttpGet("starts-with/{code}")]
        public async Task<IActionResult> StartsWith([FromServices] Core.Models.DBContext dbContext, string code)
        {
            var data = await dbContext.GameConfigs.Where(x => code.ToLower().StartsWith(x.Code)).ToListAsync();

            return Ok(data);
        }

        #endregion /starts-with/:code

        #region /patch-notes/:code/:type

        [HttpGet("patch-notes/{code}/{type}")]
        public async Task<IActionResult> PatchNotes([FromServices] Services.IPatchNotes patchNotes, string code, string type)
        {
            return Ok(await patchNotes.Get(code, type));
        }

        #endregion /patch-notes/:code/:type

        #region /patch-notes/:code/:type/rss
        [ResponseCache(Duration = 1200)]
        [HttpGet("patch-notes/{code}/{type}/rss")]
        public async Task<IActionResult> PatchNotesRss([FromServices] Services.IPatchNotes patchNotes, [FromServices] IGameParents gameParents, string code, string type)
        {
            var notes = await patchNotes.All(code, type);

            var parent = await gameParents.Get(code);

            var feed = new SyndicationFeed($"{parent.Name} {Helpers.GameTypeFixer.Fix(type)} Patch Notes",
                $"Patch notes for {parent.Name} {Helpers.GameTypeFixer.Fix(type)}",
                new Uri("https://blizztrack.com"),
                $"https://blizztrack.com/rss/{parent.Slug}/{type}", DateTime.Now)
            {
                Copyright = new TextSyndicationContent($"{DateTime.Now.Year} Mitchel Sellers")
            };

            var items = new List<SyndicationItem>();
            foreach (var item in notes)
            {
                var postUrl = $"https://blizztrack.com/patch-notes/{parent.Slug}/{type}?build_time={item.Created.Ticks}";
                var title = $"Posted: {item.Created}";
                var description = $"New patch notes discovered on {item.Created}";
                var f = new SyndicationItem(title, description, new Uri(postUrl), postUrl, item.Created)
                {
                    PublishDate = item.Created, LastUpdatedTime = item.Updated, Id = item.Created.Ticks.ToString()
                };


                f.Categories.Add(new SyndicationCategory(type));
                items.Add(f);
            }

            feed.Items = items;
            var settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                NewLineHandling = NewLineHandling.None,
                NewLineOnAttributes = false,
                Indent = true
            };
            await using var stream = new MemoryStream();
            await using (var xmlWriter = XmlWriter.Create(stream, settings))
            {
                var rssFormatter = new Rss20FeedFormatter(feed, false);
                rssFormatter.WriteTo(xmlWriter);
                await xmlWriter.FlushAsync();
            }
            return File(stream.ToArray(), "application/rss+xml; charset=utf-8");
        }

        #endregion /patch-notes/:code/:type/rss
        
        #region /trigger-alert/:code

        [HttpGet("trigger-alert/{code}")]
        public async Task<IActionResult> TriggerAlert(string code, [FromQuery(Name = "type")] NotificationType notifyType = NotificationType.Versions)
        {
            switch (notifyType)
            {
                case NotificationType.Versions:
                    var latestVersions = await _versions.Latest(code);
                    await _redisDatabase.PublishAsync("event_notifications", new Notification
                    {
                        NotificationType = notifyType,
                        Payload = new Dictionary<string, object>
                        {
                            { "code", code },
                            { "seqn", latestVersions.Seqn },
                            { "flags", "versions" },
                        }
                    });
                    break;
                case NotificationType.PatchNotes:
                    var latestPatchNotes = await _patchNotes.Get(code, "live");
                    await _redisDatabase.PublishAsync("event_notifications", new Notification
                    {
                        NotificationType = notifyType,
                        Payload = new Dictionary<string, object>
                        {
                            { "code", code },
                            { "flags", "live" },
                            { "index_time", latestPatchNotes.Created.Ticks }
                        }
                    });
                    break;
                default:
                    return BadRequest($"{notifyType} doesn't exist");
            }
            
            return Ok("Ok");
        }

        #endregion /trigger-alert/:code
    }
}