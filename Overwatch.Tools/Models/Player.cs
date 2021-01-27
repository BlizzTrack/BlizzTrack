using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;

namespace Overwatch.Tools.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Platform
    {
        PC,
        PSN,
        XBL
    }

    public class Player
    {
        public string Name { get; set; }
        public string Icon { get; set; }
        public Platform Platform { get; set; }
    }
}
