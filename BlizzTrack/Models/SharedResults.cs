using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
    }
}
