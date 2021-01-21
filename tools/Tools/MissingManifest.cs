﻿using Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MimeKit;
using ShellProgressBar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Threading.Tasks;
using Tooling.Attributes;

namespace Tooling.Tools
{
    [Tool(Name = "Missing Manifest", Disabled = true)]
    public class MissingManifest : ITool
    {
        private readonly DBContext _dbContext;
        private readonly ILogger<MissingManifest> _logger;

        public MissingManifest(DBContext dbContext, ILogger<MissingManifest> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Start()
        {
            await MissingSummary();
            await MissingGameManifest();
        }

        public async Task MissingSummary()
        {
            var files = Directory.EnumerateFiles("D:\\blizzard\\ribbit_data\\summary", "*.bmime", SearchOption.AllDirectories);
            var eFiles = files.GetEnumerator();

            int updateCycle = 1;

            Console.Clear();

            using var pbar = new ProgressBar(files.Count(), "main progressbar", new ProgressBarOptions
            {
                ForegroundColor = ConsoleColor.Yellow,
                ForegroundColorDone = ConsoleColor.DarkGreen,
                BackgroundColor = ConsoleColor.DarkGray,
                BackgroundCharacter = '\u2593'
            });

            while (eFiles.MoveNext())
            {
                var file = eFiles.Current;

                var options = Path.GetFileNameWithoutExtension(file).Split("-");

                pbar.Message = $"Reading: {Path.GetFileName(file)}";

                (string code, string hashtash, int seqn) = (options[0], options[1], int.Parse(options[2]));

                var fileContent = File.ReadAllText(file);
                var mail = await MimeMessage.LoadAsync(file);

                var manifest = mail.BodyParts.OfType<MimePart>().LastOrDefault();
                var body = mail.BodyParts.OfType<TextPart>().LastOrDefault();

                using StreamReader reader = new StreamReader(manifest.Content.Stream);
                string text = reader.ReadToEnd().Replace("\n", "");

                var signDate = GetSignerCert(text);
                var payload = body.Text.Split("\n").ToList();
                payload.Insert(0, "## Nothing");
                var (Value, Seqn) = BNetLib.Networking.BNetTools<List<BNetLib.Models.Summary>>.Parse(payload);

                var exist = await _dbContext.Summary.AsNoTracking().FirstOrDefaultAsync(x => x.Seqn == seqn);
                if(exist != null)
                {
                    updateCycle++;
                    exist.Raw = fileContent;
                    exist.Content = Value.ToArray();

                    _dbContext.Update(exist);
                }
                else
                {
                    updateCycle++;
                    var p = Manifest<BNetLib.Models.Summary[]>.Create(seqn, "summary", Value.ToArray());
                    if (signDate != null)  p.Indexed = signDate.Value;
                    p.Raw = fileContent;

                    _dbContext.Add(p);
                } 

                if (updateCycle > 100)
                {
                    await _dbContext.SaveChangesAsync();
                    updateCycle = 1;
                }

                pbar.Tick();
            }

            await _dbContext.SaveChangesAsync();
        }


        public async Task MissingGameManifest()
        {
            var files = Directory.EnumerateFiles("D:\\blizzard\\ribbit_data\\products", "*.bmime", SearchOption.AllDirectories);
            var eFiles = files.GetEnumerator();

            int updateCycle = 1;

            Console.Clear();

            using var pbar = new ProgressBar(files.Count(), "main progressbar", new ProgressBarOptions
            {
                ForegroundColor = ConsoleColor.Yellow,
                ForegroundColorDone = ConsoleColor.DarkGreen,
                BackgroundColor = ConsoleColor.DarkGray,
                BackgroundCharacter = '\u2593'
            });

            while (eFiles.MoveNext())
            {
                var file = eFiles.Current;

                var options = Path.GetFileNameWithoutExtension(file).Split("-");
                (string type, string code, int seqn) = (options[0], options[1], int.Parse(options[2]));

                pbar.Message = $"Reading: {Path.GetFileName(file)}";


                var fileContent = File.ReadAllText(file);
                var mail = await MimeMessage.LoadAsync(file);

                var manifest = mail.BodyParts.OfType<MimePart>().LastOrDefault();
                var body = mail.BodyParts.OfType<TextPart>().LastOrDefault();

                using StreamReader reader = new StreamReader(manifest.Content.Stream);
                string text = reader.ReadToEnd().Replace("\n", "");

                var signDate = GetSignerCert(text);
                var payload = body.Text.Split("\n").ToList();
                payload.Insert(0, "## Nothing");

                switch(type)
                {
                    case "versions" or "version":
                        {
                            var exist = await _dbContext.Versions.AsNoTracking().FirstOrDefaultAsync(x => x.Seqn == seqn && x.Code.ToLower() == code.ToLower());

                            if (exist == null)
                            {
                                var (Value, Seqn) = BNetLib.Networking.BNetTools<List<BNetLib.Models.Versions>>.Parse(payload);
                                var p = Manifest<BNetLib.Models.Versions[]>.Create(seqn, code, Value.ToArray());
                                if (signDate != null) p.Indexed = signDate.Value;
                                p.Raw = fileContent;

                                updateCycle++;
                                _dbContext.Add(p);
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(exist.Raw))
                                {
                                    exist.Raw = fileContent;
                                    _dbContext.Update(exist);
                                }
                            }
                            break;
                        }
                    case "cdn" or "cdns":
                        {
                            var exist = await _dbContext.CDN.AsNoTracking().FirstOrDefaultAsync(x => x.Seqn == seqn && x.Code.ToLower() == code.ToLower());

                            if (exist == null)
                            {
                                var (Value, Seqn) = BNetLib.Networking.BNetTools<List<BNetLib.Models.CDN>>.Parse(payload);
                                var p = Manifest<BNetLib.Models.CDN[]>.Create(seqn, code, Value.ToArray());
                                if (signDate != null) p.Indexed = signDate.Value;
                                p.Raw = fileContent;

                                updateCycle++;
                                _dbContext.Add(p);
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(exist.Raw))
                                {
                                    exist.Raw = fileContent;
                                    _dbContext.Update(exist);
                                }
                            }
                            break;
                        }
                    case "bgdl":
                        {
                            var exist = await _dbContext.BGDL.AsNoTracking().FirstOrDefaultAsync(x => x.Seqn == seqn && x.Code.ToLower() == code.ToLower());

                            if (exist == null)
                            {
                                var (Value, Seqn) = BNetLib.Networking.BNetTools<List<BNetLib.Models.BGDL>>.Parse(payload);
                                var p = Manifest<BNetLib.Models.BGDL[]>.Create(seqn, code, Value.ToArray());
                                if (signDate != null) p.Indexed = signDate.Value;
                                p.Raw = fileContent;

                                updateCycle++;
                                _dbContext.Add(p);
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(exist.Raw))
                                {
                                    exist.Raw = fileContent;
                                    _dbContext.Update(exist);
                                }
                            }
                            break;
                        }
                }

                if(updateCycle > 100)
                {
                    await _dbContext.SaveChangesAsync();
                    updateCycle = 1;
                }

                pbar.Tick();
            }

            await _dbContext.SaveChangesAsync();
        }

        private DateTime? GetSignerCert(String b64Signature)
        {
            byte[] binarySignature = Convert.FromBase64String(b64Signature);

            SignedCms cms = new SignedCms();
            cms.Decode(binarySignature);

            SignerInfoCollection coll = cms.SignerInfos;

            // Normally there is just the one signer certificate, which this will return
            SignerInfoEnumerator siEnum = coll.GetEnumerator();
            if (siEnum.MoveNext())
            {
                var attriubtes = siEnum.Current.SignedAttributes.GetEnumerator();
                while(attriubtes.MoveNext())
                {
                    if(attriubtes.Current.Oid.FriendlyName == "Signing Time")
                    {
                        var d = attriubtes.Current.Values.OfType<Pkcs9SigningTime>().FirstOrDefault();
                        if (d == null) return null;
                        return d.SigningTime;
                    }
                    
                }
            }

            return null;
        }
    }
}