using Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tooling.Attributes;

namespace Tooling.Tools
{
    [Tool(Name = "Fix Botched Summary Manifest")]
    class FixBotchedSummaryManifests : ITool
    {
        private readonly DBContext _dbContext;
        private readonly ILogger<FixBotchedSummaryManifests> _logger;

        public FixBotchedSummaryManifests(DBContext dbContext, ILogger<FixBotchedSummaryManifests> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Start()
        {
            await BotchedSummaryManifest();
        }

        public async Task BotchedSummaryManifest()
        {
            var summeries = await _dbContext.Summary.OrderByDescending(x => x.Seqn).ToListAsync();
            var tempFile = System.IO.Path.GetTempFileName();

            int updateCycle = 1;

            foreach(var summary in summeries)
            {
                var code = summary.Code;
                var seqn = summary.Seqn;

                System.IO.File.WriteAllText(tempFile, summary.Raw);

                var mail = await MimeKit.MimeMessage.LoadAsync(tempFile);

                var manifest = mail.BodyParts.OfType<MimePart>().LastOrDefault();
                var body = mail.BodyParts.OfType<TextPart>().LastOrDefault();

                using StreamReader reader = new StreamReader(manifest.Content.Stream);
                string text = reader.ReadToEnd().Replace("\n", "");

                var payload = body.Text.Split("\n").ToList();
                payload.Insert(0, "## Nothing");
                var summaryData = BNetLib.Networking.BNetTools<List<BNetLib.Models.Summary>>.Parse(payload);


                updateCycle++;
                summary.Content = summaryData.Value.ToArray();

                _dbContext.Update(summary);

                _logger.LogInformation($"Fixed botched summary {seqn}");

                if (updateCycle > 100)
                {
                    await _dbContext.SaveChangesAsync();
                    updateCycle = 1;
                }
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}
