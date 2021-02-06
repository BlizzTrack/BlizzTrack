using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Runtime.Serialization;

namespace BlizzTrack.Models
{
    public class SharedResults
    {
        public class SeqnItem
        {
            /// <summary>
            ///     Summary Seqn
            /// </summary>
            public int Seqn { get; set; }

            /// <summary>
            ///     Date indexed
            /// </summary>
            public DateTime Indexed { get; set; }

            /// <summary>
            ///     Link to view seqn
            /// </summary>
            public string View { get; set; }
        }


        [DataContract]
        [JsonConverter(typeof(StringEnumConverter))]
        public enum RelationTypes
        {
            PatchNotes,
            View,
            Seqn
        }

    }
}
