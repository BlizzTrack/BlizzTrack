using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNetLib.Catalogs.Models
{
    public class Installs
    {
        public string TactId { get; set; }

        public string TactCode { get; set; }

        public Installs() { }

        public Installs(string tactId, string tactCode)
        {
            TactCode = tactCode;
            TactId = tactId;
        }
    }
}
