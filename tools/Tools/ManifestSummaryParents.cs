using Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShellProgressBar;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tooling.Attributes;

namespace Tooling.Tools
{
    [Tool(Name = "Manifest Summary Parent Seqn", Disabled = true, Order = 1)]
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
            Console.Clear();

            var summeries = await _dbContext.Summary.OrderByDescending(x => x.Seqn).ToListAsync();

            var options = new ProgressBarOptions
            {
                ForegroundColor = ConsoleColor.Yellow,
                ForegroundColorDone = ConsoleColor.DarkGreen,
                BackgroundColor = ConsoleColor.DarkGray,
                BackgroundCharacter = '\u2593'
            };
            var childOptions = new ProgressBarOptions
            {
                ForegroundColor = ConsoleColor.Green,
                BackgroundColor = ConsoleColor.DarkGreen,
                CollapseWhenFinished = true,
                BackgroundCharacter = '\u2593'
            };

            using var pbar = new ProgressBar(summeries.Count, "main progressbar", options);

            var id = 1;
            foreach (var summary in summeries)
            {
                pbar.Message = $"Processing Summary {summary.Seqn} {id}/{pbar.MaxTicks}";
                using var child = pbar.Spawn(summary.Content.Length, "child actions", childOptions);

                var gameId = 1;
                foreach (var item in summary.Content)
                {
                    child.Message = $"Processing Game {item.Product}({item.Seqn}) summary({summary.Seqn}) {gameId}/{child.MaxTicks}";
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

                    gameId++;
                    child.Tick();
                }

                // _logger.LogInformation($"Updated Parent info for summary {summary.Seqn}");
                pbar.Message = $"Saving Summary {summary.Seqn} {id}/{pbar.MaxTicks}";

                id++;
                pbar.Tick();

                await _dbContext.SaveChangesAsync();
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}
