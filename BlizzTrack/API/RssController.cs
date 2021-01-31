using BlizzTrack.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BlizzTrack.API
{
    [ApiExplorerSettings(GroupName = "RSS Feeds")]
    [Route("rss")]
    [ApiController]
    [Produces("application/rss+xml")]
    public class RssController : ControllerBase
    {
        private readonly Core.Services.IGameParents _gameParents;
        private readonly Services.IPatchnotes _patchnotes;

        public RssController(IPatchnotes patchnotes, Core.Services.IGameParents gameParents)
        {
            _patchnotes = patchnotes;
            _gameParents = gameParents;
        }

        /// <summary>
        ///     Get RSS feed for given game
        /// </summary>
        /// <returns>Returns the RSS feed for the given game</returns>
        /// <response code="200">Returns valid RSS feed</response>
        /// <param name="slug">The game slug (EX: overwatch)</param>
        /// <param name="type">The game type (EX: ptr)</param>
        [ResponseCache(Duration = 60 * 5)]
        [HttpGet("{slug}/{type}")]
        public async Task<IActionResult> Get(string slug, string type)
        {

            var parent = await _gameParents.Get(slug);

            if (parent == null || !parent.PatchNoteAreas.Contains(type.ToLower())) return NotFound();

            var notes = await _patchnotes.All(parent.Code, type);
            if (notes == null || notes.Count <= 0) return NotFound();


            var feed = new SyndicationFeed($"{parent.Name} {Helpers.GameTypeFixer.Fix(type)} Patch Notes", $"Patch notes for {parent.Name} {Helpers.GameTypeFixer.Fix(type)}", new Uri("https://blizztrack.com"), $"https://blizztrack.com/rss/{parent.Slug}/{type}", DateTime.Now);

            var items = new List<SyndicationItem>();
            foreach (var item in notes)
            {
                // TODO: See if Url.Action supports pages
                var postUrl = $"https://blizztrack.com/patch-notes/{parent.Slug}/{type}?build_time={item.Created.Ticks}";
                var title = $"Posted: {item.Created}";
                var description = $"New patch notes discovered on {item.Created}";
                var f = new SyndicationItem(title, description, new Uri(postUrl), postUrl, item.Created)
                {
                    PublishDate = item.Created,
                    LastUpdatedTime = item.Updated,
                    Id = item.Created.Ticks.ToString()
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
            using var stream = new MemoryStream();
            using (var xmlWriter = XmlWriter.Create(stream, settings))
            {
                var rssFormatter = new Rss20FeedFormatter(feed, false);
                rssFormatter.WriteTo(xmlWriter);
                xmlWriter.Flush();
            }
            return File(stream.ToArray(), "application/rss+xml; charset=utf-8");
        }
    }
}
