using System;
using System.Threading.Tasks;
using Core.Models;
using Core.Services;
using Dasync.Collections;
using ShellProgressBar;
using Tooling.Attributes;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Tooling.Tools
{
    [Tool(Name = "Manifest Relationships", Order = 1, Disabled = false)]
    public class ManifestRelationships : ITool
    {
        private readonly DBContext _dbContext;
        private readonly IGameConfig _gameConfig;

        public ManifestRelationships(DBContext dbContext, IGameConfig gameConfig)
        {
            _dbContext = dbContext;
            _gameConfig = gameConfig;
        }
        
        public async Task Start()
        {
            var configs = await _gameConfig.All();
            
            using var pbar = new ProgressBar(3, "main progressbar", new ProgressBarOptions
            {
                ForegroundColor = ConsoleColor.Yellow,
                ForegroundColorDone = ConsoleColor.DarkGreen,
                BackgroundColor = ConsoleColor.DarkGray,
                BackgroundCharacter = '\u2593'
            });

            var versions = await _dbContext.Versions.OrderByDescending(x => x.Seqn)
                .Where(x => string.IsNullOrEmpty(x.ConfigId)).ToListAsync();
    
            var versionBar = pbar.Spawn(versions.Count, "Versions");
            foreach (var version in versions)
            {
                versionBar.Message = $"Updating version {version.Code} {version.Seqn}";

                version.Config = configs.FirstOrDefault(x => x.Code == version.Code);
                _dbContext.Update(version);

                versionBar.Tick();
            }
            await _dbContext.SaveChangesAsync();
            
            var cdns = await _dbContext.CDN.OrderByDescending(x => x.Seqn).Where(x => string.IsNullOrEmpty(x.ConfigId)).ToListAsync();
            
            var cdnBar = pbar.Spawn(cdns.Count, "CDNs");
            foreach (var version in cdns)
            {
                cdnBar.Message = $"Updating cdns {version.Code}";

                version.Config = configs.FirstOrDefault(x => x.Code == version.Code);
                _dbContext.Update(version);

                cdnBar.Tick();
            }
            await _dbContext.SaveChangesAsync();
            
            var bgdl = await _dbContext.BGDL.OrderByDescending(x => x.Seqn).Where(x => string.IsNullOrEmpty(x.ConfigId)).ToListAsync();
            var bgdlBar = pbar.Spawn(bgdl.Count, "BGDL");
            foreach (var version in bgdl)
            {
                bgdlBar.Message = $"Updating bgdl {version.Code}";

                version.Config = configs.FirstOrDefault(x => x.Code == version.Code);
                _dbContext.Update(version);
                bgdlBar.Tick();
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}