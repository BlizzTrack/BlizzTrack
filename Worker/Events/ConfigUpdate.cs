using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Worker.Events
{
    public class ConfigUpdate
    {
        public string Code { get; set; }

        public string Hash { get; set; }
    }
}
