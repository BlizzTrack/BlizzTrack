using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Overwatch.Tools.Models
{
    [DataContract]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SearchMode
    {
        LevelHighToLow,
        LevelLowToHigh,
    }

    [DataContract]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SearchPlatform
    {
        PC,
        PSN,
        XBL,
        NINTENDOSWITCH
    }

    public class PlayerSearchResult<T>
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("urlName")]
        public string UrlName { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("level")]
        public int? Level { get; set; }

        [JsonProperty("playerLevel")]
        public int? PlayerLevel { get; set; }

        [JsonProperty("isPublic")]
        public bool IsPublic { get; set; }

        [JsonProperty("platform")]
        public string Platform { get; set; }

        [JsonProperty("portrait")]
        public T Portrait { get; set; }
    }
}
