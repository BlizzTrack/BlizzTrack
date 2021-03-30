using System.Collections.Generic;

namespace BlizzTrack.Helpers
{
    public static class GameTypeFixer
    {
        private static readonly Dictionary<string, string> Names = new()
        {
            { "version", "Version" },
            { "versions", "Versions" },
            { "cdn", "CDN" },
            { "cdns", "CDNs" },
            { "bgdl", "BGDL" },
            { "experimental", "Experimental" },
            { "retail", "Retail" },
            { "live", "Live" },
            { "ptr", "PTR" },
            { "beta", "Beta" },
        };

        public static string Fix(string type)
        {
            return Names.ContainsKey(type.ToLower()) ? Names[type.ToLower()] : type;
        }
    }
}
