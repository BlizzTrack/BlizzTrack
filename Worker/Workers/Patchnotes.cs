using Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Worker.Workers
{
    internal class PatchnotesHosted : IHostedService
    {
        private readonly IServiceProvider serviceProvider;

        public PatchnotesHosted(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(() =>
            {
                var c = ActivatorUtilities.CreateInstance<Patchnotes>(serviceProvider);
                c.Run(cancellationToken);
            }, cancellationToken);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    public class Patchnotes
    {
        private readonly ILogger<Patchnotes> _logger;
        private readonly IServiceScopeFactory _serviceScope;

        public Patchnotes(IServiceScopeFactory serviceScope, ILogger<Patchnotes> logger)
        {
            _serviceScope = serviceScope;
            _logger = logger;
        }

        internal async void Run(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var stopWatch = Stopwatch.StartNew();

                using var sc = _serviceScope.CreateScope();
                var _gameParents = sc.ServiceProvider.GetRequiredService<Core.Services.IGameParents>();
                var _dbContext = sc.ServiceProvider.GetRequiredService<DBContext>();

                var parents = await _gameParents.All();

                var dataItems = new List<PatchNote>();
                foreach (var parent in parents.Where(x => x.PatchNoteAreas?.Count > 0))
                {
                    List<PatchNote> data = parent.PatchNoteTool switch
                    {
                        "overwatch" => await OverwatchPatchNotes(parent),
                        "legacy" => await LegacyPatchNotes(parent),
                        _ => null
                    };

                    dataItems.AddRange(data);
                }

                var hasChanges = false;
                foreach (var item in dataItems)
                {
                    var exist = await _dbContext.PatchNotes.FirstOrDefaultAsync(x => x.Code == item.Code && x.Type == item.Type && x.Created == item.Created, cancellationToken: cancellationToken);
                    if (exist != null)
                    {
                        if (exist.Updated != item.Updated)
                        {
                            exist.Updated = item.Updated;
                            exist.Body = item.Body;

                            _dbContext.Update(exist);
                            hasChanges = true;
                        }
                    }
                    else
                    {
                        _dbContext.Add(item);
                        hasChanges = true;
                    }
                }

                if (hasChanges)
                {
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }

                TimeSpan ts = stopWatch.Elapsed;
                string elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
                _logger.LogDebug($"Patch Notes took {elapsedTime}");

                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
            }
        }

        private static async Task<List<PatchNote>> OverwatchPatchNotes(GameParents parent)
        {
            var res = new List<PatchNote>();

            foreach (var code in parent.PatchNoteAreas)
            {
                var url = $"https://cdn.blz-contentstack.com/v3/content_types/game_update/entries?desc=post_date&environment=prod&query={{\"type\":\"{code}\", \"expired\":false}}&limit=1&locale=en-us";

                var headers = new Dictionary<string, string>()
                {
                    { "api_key", "blt43efdd4acc4bdcb2" },
                    { "access_token", "cs10ce60130ad4ae4fcacf3344" }
                };

                var data = await BNetLib.Http.RemoteJson.Get<BNetLib.Models.Patchnotes.Overwatch.Root>(url, headers);

                if (data.Item1.Entries.Count > 0)
                {
                    var f = data.Item1.Entries.First();

                    var note = PatchNote.Create(JsonConvert.SerializeObject(f));
                    note.Created = f.PostDate;
                    note.Updated = f.UpdatedAt;

                    note.Type = code;
                    note.Code = parent.Code;

                    res.Add(note);
                }
            }

            return res;
        }

        private static async Task<List<PatchNote>> LegacyPatchNotes(GameParents parent)
        {
            var res = new List<PatchNote>();

            foreach (var code in parent.PatchNoteAreas)
            {
                var url = $"https://cache-cms-ext-us.battle.net/system/cms/oauth/api/patchnote/list?program={(parent.PatchNoteCode ?? parent.Code)}&region=us&locale=enUS&type={code}&page=1&pageSize=1&orderBy=buildNumber";

                var data = await BNetLib.Http.RemoteJson.Get<BNetLib.Models.Patchnotes.Legacy.Root>(url);

                var f = data.Item1.PatchNotes.First();

                var note = PatchNote.Create(JsonConvert.SerializeObject(f));
                note.Created = DateTimeOffset.FromUnixTimeMilliseconds(f.Publish).UtcDateTime;
                if (f.Updated > 0)
                {
                    note.Updated = DateTimeOffset.FromUnixTimeMilliseconds(f.Updated).UtcDateTime;
                }

                note.Type = code;
                note.Code = parent.Code;

                res.Add(note);
            }

            return res;
        }
    }
}