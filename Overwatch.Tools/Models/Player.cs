using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Overwatch.Tools.Models
{
    [DataContract]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Endorsement
    {
        GoodTeammate,
        Sportsmanship,
        Shotcaller
    }

    [DataContract]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Platform
    {
        PC,
        PSN,
        XBL
    }

    public class Player<T>
    {
        /// <summary>
        ///     Battle tag
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Player Icon
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        ///     Platform which the data is from
        /// </summary>
        public Platform Platform { get; set; }

        /// <summary>
        ///     Data requested
        /// </summary>
        public T Data { get; set; }
    }

    public class Endorsements
    {
        public int Level { get; set; }

        [Newtonsoft.Json.JsonProperty("endorsements")]
        public System.Collections.Generic.Dictionary<Endorsement, float> EndorsementsCount { get; set; }
    }
}
