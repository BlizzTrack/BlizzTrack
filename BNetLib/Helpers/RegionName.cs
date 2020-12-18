﻿using System.Collections.Generic;

namespace BNetLib.Helpers
{
    public static class RegionName
    {
        private readonly static Dictionary<string, string> _regionNames = new Dictionary<string, string>()
        {
            { "US", "Americas" },
            { "EU", "Europe" },
            { "KR", "Asia" },
            { "CN", "China" },
            { "TW", "Southeast Asia (TW)" },
            { "SG", "Southeast Asia (SG)" },
            { "XX", "Public Test Realm" },
            { "BETA", "Beta" },
            { "ANDROID_CN_HUAWEI", "Huawei App Store China" },
            { "ANDROID_GOOGLE", "Google Play" },
            { "ANDROID_CN", "Direct APK" },
            { "ANDROID_AMAZON", "Amazon App Store" },
            { "IOS", "Apple App Store" },
            { "IOS_CN", "Apple App Store China" },
        };

        public static string Get(string code)
        {
            if (_regionNames.TryGetValue(code.ToUpper(), out var name)) return name;

            return code;
        }
    }
}
