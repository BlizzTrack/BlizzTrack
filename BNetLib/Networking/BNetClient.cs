﻿using BNetLib.Models;
using BNetLib.Networking.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BNetLib.Networking
{
    public class BNetClient
    {
        private readonly string _serverUrl;

        public BNetClient(ServerRegion region = ServerRegion.US)
        {
            _serverUrl = $"{region.ToString().ToLower()}.version.battle.net";
        }

        public async Task<ClientResult<BNetLib.Models.Versions>> Versions(string code)
        {
            return await Do<BNetLib.Models.Versions>(new VersionCommand(code));
        }

        public async Task<ClientResult<BNetLib.Models.BGDL>> BGDL(string code)
        {
            return await Do<BNetLib.Models.BGDL>(new BGDLCommand(code));
        }

        public async Task<ClientResult<BNetLib.Models.CDN>> CDN(string code)
        {
            return await Do<BNetLib.Models.CDN>(new CDNCommand(code));
        }

        public async Task<ClientResult<BNetLib.Models.Summary>> Summary()
        {
            return await Do<BNetLib.Models.Summary>(new SummaryCommand());
        }

        public async Task<ClientResult<T>> Do<T>(AbstractCommand command) where T : NGPD, new() => await Do<T>(command.ToString());

        public async Task<ClientResult<T>> Do<T>(string command) where T : NGPD, new()
        {
            using var client = new TcpClient();
            await client.ConnectAsync(_serverUrl, 1119);

            await using var ms = client.GetStream();

            var payload = Encoding.UTF8.GetBytes($"{command}\r\n");
            await ms.WriteAsync(payload.AsMemory(0, payload.Length));

            if (ms.CanRead)
            {
                using var reader = new StreamReader(ms, Encoding.UTF8);
                try
                {
                    var result = await reader.ReadToEndAsync();

                    /// From TactLib -> https://github.com/overtools/TACTLib/blob/7d2ecbc98b83a315ea599fd519403fa0d8b24dce/TACTLib/Protocol/Ribbit/RibbitClient.cs
                    var text = result.Split("\n");
                    var boundary = text.FirstOrDefault(x => x.Trim().StartsWith("Content-Type:"))?.Split(';').FirstOrDefault(x => x.Trim().StartsWith("boundary="))?.Split('"')[1].Trim();
                    var data = text.SkipWhile(x => x.Trim() != "--" + boundary).Skip(1).TakeWhile(x => x.Trim() != "--" + boundary).Skip(1);

                    var outData = BNetTools.Parse<T>(data);

                    return new ClientResult<T>()
                    {
                        Payload = outData.Value,
                        Seqn = outData.Seqn,
                        Raw = result
                    };
                }
                finally
                {
                    client.Close();
                    ms.Close();
                }
            }

            client.Close();
            ms.Close();
            return default;
        }

        public async Task<string> Call(string command)
        {
            using var client = new TcpClient();
            await client.ConnectAsync(_serverUrl, 1119);

            await using var ms = client.GetStream();

            var payload = Encoding.UTF8.GetBytes($"{command}\r\n");
            await ms.WriteAsync(payload.AsMemory(0, payload.Length));

            if (ms.CanRead)
            {
                using var reader = new StreamReader(ms, Encoding.UTF8);
                try
                {
                    var result = await reader.ReadToEndAsync();
                    return result;
                }
                finally
                {
                    client.Close();
                    ms.Close();
                }
            }

            client.Close();
            ms.Close();
            return string.Empty;
        }
    }
}