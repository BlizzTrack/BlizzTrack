using System;
using System.Collections.Generic;
using System.Text;

namespace BNetLib.Networking.Commands
{
    public class CDNCommand : AbstractCommand
    {
        private readonly string _code;

        public CDNCommand(string code)
        {
            _code = code;
        }

        public override string ToString()
        {
            return CreateCommand("products", _code, "cdns");
        }
    }
}
