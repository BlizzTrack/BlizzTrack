using Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MimeKit;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tooling.Attributes;

namespace Tooling.Tools
{
    [Tool(Name = "Fix Botched Summary Manifest", Disabled = true)]
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
            var summaries = await _dbContext.Summary.OrderByDescending(x => x.Seqn).ToListAsync();
            var tempFile = Path.GetTempFileName();

            var updateCycle = 1;

            foreach(var summary in summaries)
            {
                var seqn = summary.Seqn;

                await File.WriteAllTextAsync(tempFile, summary.Raw);

                var mail = await MimeMessage.LoadAsync(tempFile);

                var manifest = mail.BodyParts.OfType<MimePart>().LastOrDefault();
                var body = mail.BodyParts.OfType<TextPart>().LastOrDefault();

                if (manifest != null)
                {
                    using var reader = new StreamReader(manifest.Content.Stream);
                    var replace = (await reader.ReadToEndAsync()).Replace("\n", "");
                }

                var payload = body?.Text.Split("\n").ToList();
                payload?.Insert(0, "## Nothing");
                var (value, Seqn) = BNetLib.Networking.BNetTools.Parse<BNetLib.Models.Summary>(payload);

                updateCycle++;
                summary.Content = value.ToArray();

                _dbContext.Update(summary);

                _logger.LogInformation($"Fixed botched summary {seqn}");

                if (updateCycle <= 100) continue;
                await _dbContext.SaveChangesAsync();
                updateCycle = 1;
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}
