using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace BNetLib.test
{
    [TestClass]
    public class BNetTools
    {
        [TestMethod]
        [DataRow(@"## nothing
Region!STRING:0|BuildConfig!HEX:16|CDNConfig!HEX:16|KeyRing!HEX:16|BuildId!DEC:4|VersionsName!String:0|ProductConfig!HEX:16
## seqn = 431874
us|bc968f5914f05bc6b9ff67f6ed63a4d0|fb5eb6011c94e9d32e5e7c4124e416f5||8512817|bo5_TU8_8512817_2_ship|a9d8e0ab0615fcc2a919bbe840493de4
eu|b3585b8f0cbf9040ffb68d313a59c2c4|fb5eb6011c94e9d32e5e7c4124e416f5||8408002|bo5_TU7_8408002J_signed_dev_esports|a9d8e0ab0615fcc2a919bbe840493de4
kr|20745522b883e9616743d5092e479fae|fb5eb6011c94e9d32e5e7c4124e416f5||8408002|bo5_TU7_8408002J_signed_dev|a9d8e0ab0615fcc2a919bbe840493de4")]
        public void Parse(string data)
        {
            var res = BNetLib.Networking.BNetTools.Parse<BNetLib.Models.Versions>(data.Split("\n"));

            Assert.IsTrue(res.Value.GetType() == typeof(List<BNetLib.Models.Versions>), $"Failed got type {res.Value.GetType()}");
        }
    }
}
