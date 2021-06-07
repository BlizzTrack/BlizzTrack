using System.Collections.Generic;
using BNetLib.Ribbit.Models;

namespace BNetLib.Ribbit
{
    public class NgpdResult<T> where T : NGPD, new()
    {
        public int Seqn { get; set; }

        public string Raw { get; set; }

        public IList<T> Payload { get; set; }
    }
}
