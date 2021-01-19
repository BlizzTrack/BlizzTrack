using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tooling.Attributes
{
    public class ToolAttribute : Attribute
    {
        public string Name { get; set; }
        public bool Disabled { get; set; }
        public int Order { get; set; } = 0;
    }
}
