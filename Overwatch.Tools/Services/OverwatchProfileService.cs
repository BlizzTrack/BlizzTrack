using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Overwatch.Tools.Extensions;
using Overwatch.Tools.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Overwatch.Tools.Services
{
    public interface IOverwatchProfileService
    {
        Task<Player<Endorsements>> GetEndorsments(string player, Platform platform);
    }

    public class OverwatchProfileService : IOverwatchProfileService
    {
        private readonly HtmlParser _parser = new HtmlParser();

        public async Task<Player<Endorsements>> GetEndorsments(string player, Platform platform)
        {
            var result = new Player<Endorsements>()
            {
                Data = new Endorsements()
            };

            using var wc = new HttpClient
            {
                BaseAddress = new Uri("https://playoverwatch.com/en-gb/career/")
            };


            var reqUrl = platform != Platform.PC
               ? $"{platform.ToString().ToLower()}/{player}"
               : $"pc/{player.BattletagToUrlFriendlyString()}";

            using var wcRes = await wc.GetAsync(reqUrl);
            if (!wcRes.IsSuccessStatusCode) return null;
            var rsltContent = await wcRes.Content.ReadAsStringAsync();
            if (rsltContent.Contains("Profile Not Found")) return null;

            using var doc = await _parser.ParseDocumentAsync(rsltContent);

            result.Platform = platform;
            result.Name = platform != Platform.PC ? player : player.Replace("-", "#");

            result.Icon = PortraitImage(doc);
            result.Data.EndorsementsCount = Endorsements(doc);
            result.Data.Level = EndorsementLevel(doc);

            return result;
        }
        private static string PortraitImage(IHtmlDocument doc) => doc.QuerySelector(".player-portrait").GetAttribute("src");

        private static int EndorsementLevel(IHtmlDocument doc)
        {
            int.TryParse(doc.QuerySelector("div.EndorsementIcon-tooltip div.u-center")?.TextContent, out var parsedEndorsementLevel);
            return parsedEndorsementLevel;
        }

        private static Dictionary<Endorsement, float> Endorsements(IHtmlDocument doc)
        {
            var contents = new Dictionary<Endorsement, float>();

            var innerContent = doc.QuerySelector("div.endorsement-level");

            if (innerContent != null)
            {
                foreach (var endorsement in innerContent.QuerySelectorAll("svg"))
                {
                    var dataValue = endorsement.GetAttribute("data-value");

                    if (dataValue != null)
                    {
                        var className = endorsement.GetAttribute("class");
                        // parse the endorsement type out of the class name
                        const string endorsementTypeSeparator = "--";
                        var endorsementName = className.Substring(className.IndexOf(endorsementTypeSeparator, StringComparison.Ordinal) + endorsementTypeSeparator.Length);
                        contents.Add(ParseEndorsementName(endorsementName), float.Parse(dataValue));
                    }
                }
            }
            return contents;
        }

        private static Endorsement ParseEndorsementName(string input)
        {
            switch (input)
            {
                case "teammate":
                    return Endorsement.GoodTeammate;
                case "sportsmanship":
                    return Endorsement.Sportsmanship;
                case "shotcaller":
                    return Endorsement.Shotcaller;
                default:
                    return Endorsement.GoodTeammate;
            }
        }
    }
}
