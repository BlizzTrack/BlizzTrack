using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNetLib.Models
{
    public class ClientResult<T> where T : NGPD, new()
    {
        public int Seqn { get; set; }
        public string Raw { get; set; }

        public IList<T> Payload { get; set; }
    }
}
