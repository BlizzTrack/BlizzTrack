using System;
using System.Collections.Generic;
using System.Linq;
using BNetLib.Extensions;
using BNetLib.Ribbit.Models;

namespace BNetLib.Ribbit
{
    internal enum KeyTypeEnum
    {
        Dec,
        String,
        Hex
    }

    /// <summary>
    ///     This is a Key, Type system. So this let's you see what type is linked to what key
    ///     EX: "BuildId!DEC:4", this in-turn becomes BuildId -> Dec
    /// </summary>
    internal class KeyType
    {
        public KeyType () { }

        public KeyType(KeyTypeEnum type, string key)
        {
            Type = type;
            Key = key;
        }

        /// <summary>
        ///     Just the Key name
        /// </summary>
        public string Key { get; }

        /// <summary>
        ///     Just the type ex: DEC (int)
        /// </summary>
        public KeyTypeEnum Type { get; }
    }

    public static class BNetTools
    {
        // TODO: Optimize this some more, there is some issues with how I designed this system
        //       the problem is we have two different loops in this code... One to load the keys and there respected types
        //       other is to parse this data manfest... We could make this smarter some how I think
        public static (IList<T> Value, int Seqn) Parse<T>(IEnumerable<string> lines) where T : NGPD, new()
        {
            var dataItems = new List<T>();

            var enumerable = lines as string[] ?? lines.ToArray();
            if (!enumerable.Any()) return (default, 0);

            var keysLine = enumerable.Skip(1).Take(1).First();
            var seqn = 0;

            var keys = (from key in keysLine.Split("|")
                select key.Split("!")
                into item
                let itemType = item.Last().Split(":").First()
                select new KeyType(Enum.Parse<KeyTypeEnum>(itemType, true), item.First())).ToList();

            foreach (var line in enumerable.Skip(2))
            {
                if (line.StartsWith("## seqn =", StringComparison.OrdinalIgnoreCase))
                {
                    var f = string.Join("", line.Where(char.IsDigit));
                    _ = int.TryParse(f, out seqn);
                    continue;
                }

                if (line.Trim().Length == 0) continue;

                var values = line.Split('|');

                var lineItem = new Dictionary<string, object>();

                for (var i = 0; i < keys.Count; i++)
                {
                    var key = keys[i];
                    var item = values[i].Trim();

                    switch (key.Type)
                    {
                        case KeyTypeEnum.Dec:
                            _ = int.TryParse(item, out var seqn1);
                            lineItem[key.Key] = seqn1;
                            continue;
                        case KeyTypeEnum.Hex or KeyTypeEnum.String:
                            if (key.Key.ToLower() == "flags")
                            {
                                lineItem[key.Key] = item == "" ? "versions" : item.Trim();
                            }
                            else
                            {
                                lineItem[key.Key] = item == "" ? "" : item.Trim();
                            }

                            break;
                    }
                }

                dataItems.Add(lineItem.ToObject<T>());
            }

            return (dataItems, seqn);
        }
    }
}