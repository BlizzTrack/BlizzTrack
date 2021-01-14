using Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Tooling.Attributes;

namespace Tooling.Tools
{
    [Tool(Name = "Raw Manfiest Files")]

    class RawManifestFiles : ITool
    {
        private readonly DBContext _dbContext;
        private readonly ILogger<RawManifestFiles> _logger;

        public RawManifestFiles(DBContext dbContext, ILogger<RawManifestFiles> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Start()
        {
            await SetRawOnSummeries();
            await SetRawOnVersions();
            await SetRawOnCDNs();
            await SetRawOnBGDL();
        }

        private async Task SetRawOnBGDL()
        {
            var versions = await _dbContext.BGDL.OrderByDescending(x => x.Seqn).Where(x => x.Raw != null || x.Raw != "").ToListAsync();

            int updateCycle = 1;
            foreach (var summary in versions)
            {
                if (!string.IsNullOrEmpty(summary.Raw)) continue;

                var url = $"products/{summary.Code}/bgdl-{summary.Code}-{summary.Seqn}.bmime";

                var raw = await RawFile(url);
                if (raw == string.Empty) continue;

                _logger.LogInformation($"Loaded raw file for: {url}");

                summary.Raw = raw;

                updateCycle++;
                if (updateCycle > 100)
                {
                    await _dbContext.SaveChangesAsync();
                    updateCycle = 1;
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        private async Task SetRawOnCDNs()
        {
            var versions = await _dbContext.CDN.OrderByDescending(x => x.Seqn).Where(x => x.Raw != null || x.Raw != "").ToListAsync();

            int updateCycle = 1;
            foreach (var summary in versions)
            {
                if (!string.IsNullOrEmpty(summary.Raw)) continue;

                var url = $"products/{summary.Code}/cdn-{summary.Code}-{summary.Seqn}.bmime";

                var raw = await RawFile(url);
                if (raw == string.Empty) continue;

                _logger.LogInformation($"Loaded raw file for: {url}");

                summary.Raw = raw;

                updateCycle++;
                if (updateCycle > 100)
                {
                    await _dbContext.SaveChangesAsync();
                    updateCycle = 1;
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        private async Task SetRawOnVersions()
        {
            var versions = await _dbContext.Versions.OrderByDescending(x => x.Seqn).Where(x => x.Raw != null || x.Raw != "").ToListAsync();

            int updateCycle = 1;
            foreach (var summary in versions)
            {
                if (!string.IsNullOrEmpty(summary.Raw)) continue;

                var url = $"products/{summary.Code}/version-{summary.Code}-{summary.Seqn}.bmime";

                var raw = await RawFile(url);
                if (raw == string.Empty) continue;

                _logger.LogInformation($"Loaded raw file for: {url}");

                summary.Raw = raw;

                updateCycle++;
                if (updateCycle > 100)
                {
                    await _dbContext.SaveChangesAsync();
                    updateCycle = 1;
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        private async Task SetRawOnSummeries()
        {
            var summaries = await _dbContext.Summary.OrderByDescending(x => x.Seqn).Where(x => x.Raw != null || x.Raw != "").ToListAsync();

            int updateCycle = 1;
            foreach (var summary in summaries)
            {
                if (!string.IsNullOrEmpty(summary.Raw)) continue;

                var url = $"summary/summary-%23-{summary.Seqn}.bmime";

                var raw = await RawFile(url);
                if (raw == string.Empty) continue;

                _logger.LogInformation($"Loaded raw file for: {url}");

                summary.Raw = raw;

                updateCycle++;
                if(updateCycle > 100)
                {
                    await _dbContext.SaveChangesAsync();
                    updateCycle = 1;
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        private async Task<string> RawFile(string url)
        {
            using var wc = new WebClient();

            try
            {
                var data = await wc.DownloadStringTaskAsync($"https://raw.githubusercontent.com/mdX7/ribbit_data/master/{url}");

                return data;
            }catch(Exception)
            {
                return string.Empty;
            }
        }
    }
}
