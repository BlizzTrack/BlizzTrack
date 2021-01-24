using System.Collections.Generic;

namespace BNetLib.Models
{
    public class ClientResult<T> where T : NGPD, new()
    {
        public int Seqn { get; set; }

        public string Raw { get; set; }

        public IList<T> Payload { get; set; }
    }
}
