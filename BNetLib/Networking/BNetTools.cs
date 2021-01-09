using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BNetLib.Networking
{
    internal class KeyType
    {
        public KeyType(string type, string key)
        {
            Type = type;
            Key = key;
        }

        public string Key { get; }
        public string Type { get; }
    }

    internal static class BNetTools<T>
    {
        internal static (T Value, int Seqn) Parse(IEnumerable<string> lines)
        {
            var dataItems = new List<Dictionary<string, dynamic>>();

            var keys = new List<KeyType>();
            var enumerable = lines as string[] ?? lines.ToArray();
            if (!enumerable.Any()) return (default, 0);

            var keysLine = enumerable.Skip(1).Take(1).First();
            var seqn = 0;

            foreach (var key in keysLine.Split("|"))
            {
                var item = key.Split("!");
                var itemType = item.Last().Split(":").First();

                keys.Add(new KeyType(itemType, item.First()));
            }

            foreach (var line in enumerable.Skip(2))
            {
                if (line.StartsWith("## seqn ="))
                {
                    var f = line.Replace("## seqn =", "").Trim();
                    _ = int.TryParse(f, out var seqn1);
                    seqn = seqn1;
                    continue;
                }

                if (line.Trim().Length == 0) continue;

                var values = line.Split('|');

                var lineItem = new Dictionary<string, dynamic>();

                for (var i = 0; i < keys.Count; i++)
                {
                    var key = keys[i];
                    var item = values[i];

                    if (key.Type.Equals("dec", StringComparison.CurrentCultureIgnoreCase))
                    {
                        _ = int.TryParse(item, out var seqn1);
                        lineItem[key.Key] = seqn1;
                        continue;
                    }

                    lineItem[key.Key] = item == "" ? "versions" : item;
                }

                dataItems.Add(lineItem);
            }

            // FIXME: Make this not relay on json to convert between the objects... This feels wrong
            var ff = JsonConvert.SerializeObject(dataItems);

            return (JsonConvert.DeserializeObject<T>(ff), seqn);
        }
    }
}