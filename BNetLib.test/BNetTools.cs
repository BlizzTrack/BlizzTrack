using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using BNetLib.Ribbit.Models;

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
        public void VersionParse(string data)
        {
            var res = Ribbit.BNetTools.Parse<Versions>(data.Split("\n"));

            Assert.IsTrue(res.Value.GetType() == typeof(List<Versions>), $"Failed got type {res.Value.GetType()}");
        }

        [TestMethod]
        [DataRow(@"## nothing
Region!STRING:0|BuildConfig!HEX:16|CDNConfig!HEX:16|KeyRing!HEX:16|BuildId!DEC:4|VersionsName!String:0|ProductConfig!HEX:16
## seqn = 431874
us|bc968f5914f05bc6b9ff67f6ed63a4d0|fb5eb6011c94e9d32e5e7c4124e416f5||8512817|bo5_TU8_8512817_2_ship|a9d8e0ab0615fcc2a919bbe840493de4
eu|b3585b8f0cbf9040ffb68d313a59c2c4|fb5eb6011c94e9d32e5e7c4124e416f5||8408002|bo5_TU7_8408002J_signed_dev_esports|a9d8e0ab0615fcc2a919bbe840493de4
kr|20745522b883e9616743d5092e479fae|fb5eb6011c94e9d32e5e7c4124e416f5||8408002|bo5_TU7_8408002J_signed_dev|a9d8e0ab0615fcc2a919bbe840493de4")]
        public void VersionBuildId(string data)
        {
            var res = Ribbit.BNetTools.Parse<Versions>(data.Split("\n"));

            var first = res.Value.FirstOrDefault(x => x.Region == "us");
            Assert.IsTrue(first.Buildid == 8512817, $"Failed {nameof(first.Buildid)} got {first.Buildid} wanted 8512817");
        }

        [TestMethod]
        [DataRow(@"## nothing
Name!STRING:0|Path!STRING:0|Hosts!STRING:0|Servers!STRING:0|ConfigPath!STRING:0
## seqn = 384642
eu|tpr/ovw|level3.blizzard.com eu.cdn.blizzard.com|http://eu.cdn.blizzard.com/?maxhosts=4 http://level3.blizzard.com/?maxhosts=4 https://blzddist1-a.akamaihd.net/?fallback=1&maxhosts=4 https://eu.cdn.blizzard.com/?fallback=1&maxhosts=4 https://level3.ssl.blizzard.com/?fallback=1&maxhosts=4|tpr/configs/data
kr|tpr/ovw|level3.blizzard.com kr.cdn.blizzard.com blizzard.gcdn.cloudn.co.kr|http://blizzard.gcdn.cloudn.co.kr/?maxhosts=4 http://kr.cdn.blizzard.com/?maxhosts=4 http://level3.blizzard.com/?maxhosts=4 https://blizzard.gcdn.cloudn.co.kr/?fallback=1&maxhosts=4 https://blzddistkr1-a.akamaihd.net/?fallback=1&maxhosts=4 https://kr.cdn.blizzard.com/?fallback=1&maxhosts=4 https://level3.ssl.blizzard.com/?fallback=1&maxhosts=4|tpr/configs/data
cn|tpr/ovw|client02.pdl.wow.battlenet.com.cn client05.pdl.wow.battlenet.com.cn|http://client02.pdl.wow.battlenet.com.cn/?maxhosts=4 http://client05.pdl.wow.battlenet.com.cn/?maxhosts=4 https://blzddist1-a.akamaihd.net/?fallback=1&maxhosts=4 https://client02.pdl.wow.battlenet.com.cn/?fallback=1&maxhosts=4 https://client05.pdl.wow.battlenet.com.cn/?fallback=1&maxhosts=4|tpr/configs/data
us|tpr/ovw|level3.blizzard.com us.cdn.blizzard.com|http://level3.blizzard.com/?maxhosts=4 http://us.cdn.blizzard.com/?maxhosts=4 https://blzddist1-a.akamaihd.net/?fallback=1&maxhosts=4 https://level3.ssl.blizzard.com/?fallback=1&maxhosts=4 https://us.cdn.blizzard.com/?fallback=1&maxhosts=4|tpr/configs/data
tw|tpr/ovw|level3.blizzard.com us.cdn.blizzard.com|http://level3.blizzard.com/?maxhosts=4 http://us.cdn.blizzard.com/?maxhosts=4 https://blzddist1-a.akamaihd.net/?fallback=1&maxhosts=4 https://level3.ssl.blizzard.com/?fallback=1&maxhosts=4 https://us.cdn.blizzard.com/?fallback=1&maxhosts=4|tpr/configs/data")]
        public void CDNParse(string data)
        {
            var res = Ribbit.BNetTools.Parse<CDN>(data.Split("\n"));

            var first = res.Value.FirstOrDefault(x => x.Name == "us");
            Assert.IsTrue(first.Hosts == "level3.blizzard.com us.cdn.blizzard.com", $"Failed {nameof(first.Hosts)} got {first.Hosts} wanted level3.blizzard.com us.cdn.blizzard.com");
        }

        [TestMethod]
        [DataRow(@"## nothing
Name!STRING:0|Path!STRING:0|Hosts!STRING:0|Servers!STRING:0|ConfigPath!STRING:0
## seqn = 384642
eu|tpr/ovw|level3.blizzard.com eu.cdn.blizzard.com|http://eu.cdn.blizzard.com/?maxhosts=4 http://level3.blizzard.com/?maxhosts=4 https://blzddist1-a.akamaihd.net/?fallback=1&maxhosts=4 https://eu.cdn.blizzard.com/?fallback=1&maxhosts=4 https://level3.ssl.blizzard.com/?fallback=1&maxhosts=4|tpr/configs/data
kr|tpr/ovw|level3.blizzard.com kr.cdn.blizzard.com blizzard.gcdn.cloudn.co.kr|http://blizzard.gcdn.cloudn.co.kr/?maxhosts=4 http://kr.cdn.blizzard.com/?maxhosts=4 http://level3.blizzard.com/?maxhosts=4 https://blizzard.gcdn.cloudn.co.kr/?fallback=1&maxhosts=4 https://blzddistkr1-a.akamaihd.net/?fallback=1&maxhosts=4 https://kr.cdn.blizzard.com/?fallback=1&maxhosts=4 https://level3.ssl.blizzard.com/?fallback=1&maxhosts=4|tpr/configs/data
cn|tpr/ovw|client02.pdl.wow.battlenet.com.cn client05.pdl.wow.battlenet.com.cn|http://client02.pdl.wow.battlenet.com.cn/?maxhosts=4 http://client05.pdl.wow.battlenet.com.cn/?maxhosts=4 https://blzddist1-a.akamaihd.net/?fallback=1&maxhosts=4 https://client02.pdl.wow.battlenet.com.cn/?fallback=1&maxhosts=4 https://client05.pdl.wow.battlenet.com.cn/?fallback=1&maxhosts=4|tpr/configs/data
us|tpr/ovw|level3.blizzard.com us.cdn.blizzard.com|http://level3.blizzard.com/?maxhosts=4 http://us.cdn.blizzard.com/?maxhosts=4 https://blzddist1-a.akamaihd.net/?fallback=1&maxhosts=4 https://level3.ssl.blizzard.com/?fallback=1&maxhosts=4 https://us.cdn.blizzard.com/?fallback=1&maxhosts=4|tpr/configs/data
tw|tpr/ovw|level3.blizzard.com us.cdn.blizzard.com|http://level3.blizzard.com/?maxhosts=4 http://us.cdn.blizzard.com/?maxhosts=4 https://blzddist1-a.akamaihd.net/?fallback=1&maxhosts=4 https://level3.ssl.blizzard.com/?fallback=1&maxhosts=4 https://us.cdn.blizzard.com/?fallback=1&maxhosts=4|tpr/configs/data")]
        public void CDNSeqnParse(string data)
        {
            var res = Ribbit.BNetTools.Parse<CDN>(data.Split("\n"));

            Assert.IsTrue(res.Seqn == 384642, $"Failed {nameof(res.Seqn)} got {res.Seqn} wanted 384642");
        }
    }
}   
