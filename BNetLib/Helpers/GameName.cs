﻿using System;
using System.Collections.Generic;

namespace BNetLib.Helpers
{
    /*
     * TODO: Remove all this logic and store it in the database
     *       This should be dynamically set in a database w/o having to recompile the damn website
    */
    public static class GameName
    {
        private static readonly Dictionary<string, string> Suffix = new()
        {
            {"t", "PTR"},
            {"ptr", "PTR"},
            {"beta", "Beta"},
            {"classic", "Classic"},
            {"dev", "Dev"},
            {"dev2", "Dev 2"},
            {"indev", "Internal Dev"},
            { "a", "Alpha"},
            {"demo", "Demo"},
            {"demo2", "Demo 2"},
            {"demo3", "Demo 3"},
            {"demo4", "Demo 4"},
            {"c", "Tournament"},
            {"ceu", "Tournament (EU)"},
            {"ckr", "Tournament (KR)"},
            {"ccn", "Tournament (CN)"},
            {"c2", "Tournament 2"},
            {"c2eu", "Tournament 2 (EU)"},
            {"c2kr", "Tournament 2 (KR)"},
            {"c2cn", "Tournament 2 (CN)"},
            {"c3", "Tournament 3"},
            {"c4", "Tournament 4"},
            {"c5", "Tournament 5"},
            {"c3eu", "Tournament 3 (EU)"},
            {"c3kr", "Tournament 3 (KR)"},
            {"c3cn", "Tournament 3 (CN)"},
            {"cr", "Tournament Viewer"},
            {"cr1", "Tournament Viewer 1"},
            {"cr2", "Tournament Viewer 2"},
            {"cr3", "Tournament Viewer 3"},
            {"cr4", "Tournament Viewer 4"},
            {"cr5", "Tournament Viewer 5"},
            {"v", "Vendor"},
            {"v1", "Vendor 1"},
            {"v2", "Vendor 2"},
            {"v3", "Vendor 3"},
            {"v4", "Vendor 4"},
            {"v5", "Vendor 5"},
            {"v6", "Vendor 6"},
            {"v7", "Vendor 7"},
            {"v8", "Vendor 8"},
            {"v9", "Vendor 9"},
            {"v10", "Vendor 10"},
            {"v11", "Vendor 11"},
            {"v12", "Vendor 12"},
            {"v13", "Vendor 13"},
            {"v14", "Vendor 14"},
            {"v15", "Vendor 15"},
            {"v16", "Vendor 16"},
            {"v17", "Vendor 17"},
            {"v18", "Vendor 18"},
            {"v19", "Vendor 19"},
            {"v20", "Vendor 20"},
            {"vendor", "Vendor"},
            {"vendor2", "Vendor 2"},
            {"ms", "World Cup Viewer"},
            {"b", "Beta"},
            {"e", "Event"},
            {"e1", "Event 1"},
            {"e2", "Event 2"},
            {"e3", "Event 3"},
            {"e4", "Event 4"},
            {"e5", "Event 5"},
            {"e6", "Event 6"},
            {"e7", "Event 7"},
            {"e8", "Event 8"},
            {"e9", "Event 9"},
            {"e10", "Event 10"},
            {"event", "Event"},
            {"z", "Submission"},
            {"cn", "China"},
            {"igr", "Internet Game Room"},
            {"livetest", "Live Test"},
            {"locv1", "Localization 1"},
            {"locv2", "Localization 2"},
            {"locv3", "Localization 3"},
            {"locv4", "Localization 4"},
            { "utr", "User Test Realm" },
            {"ev", "Esports" },
            {"usr", "BOCW Event" },
            {"cdlevent", "League" },
            {"cdlstaff", "League Staff" }
        };

        private static Dictionary<string, string> Prefix { get; } = new()
        {
            {"pro", "Overwatch"},
            {"wowclassicera", "World of Warcraft: ERA Classic"},
            {"wowclassic", "World of Warcraft: BCC Classic"},
            {"wow", "World of Warcraft"},
            {"d3", "Diablo III"},
            {"hero", "Heroes of the Storm"},
            {"storm", "Heroes of the Storm (Deprecated)"},
            {"dst2", "Destiny 2"},
            {"s1", "Starcraft Remastered"},
            {"s2", "Starcraft II"},
            {"w3", "Warcraft III"},
            {"agent", "Battle.net Agent"},
            {"bna", "Battle.net App"},
            {"bts", "Bootstrapper"},
            {"catalogs", "Game Catalog"},
            {"hs", "Hearthstone"},
            {"viper", "Call of Duty: Black Ops 4"},
            {"odin", "Call of Duty: Modern Warfare"},
            {"lazr", "Call of Duty: Modern Warfare 2"},
            {"zeus", "Call of Duty: Black Ops Cold War"},
            {"fenris", "Diablo IV"},
            {"orbis", "Orbis (Unknown)"},
            {"auks", "Auks (Unknown)"},
            {"fore", "Call of Duty: Vanguard"},
            {"spot", "Spot (Unknown)"},
            {"unknown", "Unknown"},
            {"wlby", "Crash Bandicoot 4: It's About Time" },
            {"rtro", "Blizzard Arcade Collection" },
            {"osi", "Diablo II: Resurrected" }
        };

        public static string Get(string code)
        {
            code = code.ToLower().Replace("_", "");
            switch (code)
            {
                case "hsb":
                    return Prefix["hs"];

                case "hsc":
                    return Prefix["hs"] + " " + Suffix["c"];
            }

            var name = "";

            foreach (var (k, v) in Prefix)
            {
                if (!code.StartsWith(k)) continue;
                name += v;

                code = code.Replace(k, "");
                foreach (var (key, value) in Suffix)
                {
                    if (!code.Equals(key, StringComparison.CurrentCultureIgnoreCase)) continue;
                    name += " " + value;
                    break;
                }

                break;
            }

            return string.IsNullOrEmpty(name) ? code : name;
        }
    }
}