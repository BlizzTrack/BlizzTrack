using Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tooling.Attributes;

namespace Tooling.Tools
{
    [Tool(Name = "Manifest Summary Parent Seqn", Disabled = true)]
    public class ManifestSummaryParents : ITool
    {
        private readonly DBContext _dbContext;
        private readonly ILogger<ManifestSummaryParents> _logger;

        public ManifestSummaryParents(DBContext dbContext, ILogger<ManifestSummaryParents> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Start()
        {
            var summeries = await _dbContext.Summary.OrderByDescending(x => x.Seqn).ToListAsync();

            foreach(var summary in summeries)
            {
                foreach(var item in summary.Content)
                {
                    switch (item.Flags)
                    {
                        case "version" or "versions":
                            {
                                var c = await _dbContext.Versions.FirstOrDefaultAsync(x => x.Code == item.Product && x.Seqn == item.Seqn);
                                if(c != null)
                                {
                                    c.Parent = summary.Seqn;
                                    _dbContext.Update(c);
                                }
                            }
                            break;
                        case "cdn" or "cdns":
                            {
                                var c = await _dbContext.CDN.FirstOrDefaultAsync(x => x.Code == item.Product && x.Seqn == item.Seqn);
                                if (c != null)
                                {
                                    c.Parent = summary.Seqn;
                                    _dbContext.Update(c);
                                }
                            }
                            break;
                        case "bgdl":
                            {
                                var c = await _dbContext.BGDL.FirstOrDefaultAsync(x => x.Code == item.Product && x.Seqn == item.Seqn);
                                if (c != null)
                                {
                                    c.Parent = summary.Seqn;
                                    _dbContext.Update(c);
                                }
                            }
                            break;
                    }
                }

                _logger.LogInformation($"Updated Parent info for summary {summary.Seqn}");
                await _dbContext.SaveChangesAsync();
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}
