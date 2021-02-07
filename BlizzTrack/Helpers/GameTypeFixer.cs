using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace BlizzTrack.Helpers
{
    public static class GameTypeFixer
    {
        private readonly static Dictionary<string, string> Names = new Dictionary<string, string>
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
            if(Names.ContainsKey(type.ToLower()))
            {
                return Names[type.ToLower()];
            }

            return type;
        }
    }
}
