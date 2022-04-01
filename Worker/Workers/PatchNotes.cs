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
using BNetLib.PatchNotes;
using BNetLib.PatchNotes.Models;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Worker.Workers
{
    internal class PatchnotesHosted : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public PatchnotesHosted(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(() =>
            {
                var c = ActivatorUtilities.CreateInstance<PatchNotes>(_serviceProvider);
                c.Run(cancellationToken);
            }, cancellationToken);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    public class PatchNotes
    {
        private readonly ILogger<PatchNotes> _logger;
        private readonly IRedisDatabase _redisDatabase;
        private readonly IServiceScopeFactory _serviceScope;

        public PatchNotes(IServiceScopeFactory serviceScope, ILogger<PatchNotes> logger, IRedisDatabase redisDatabase)
        {
            _serviceScope = serviceScope;
            _logger = logger;
            _redisDatabase = redisDatabase;
        }

        internal async void Run(CancellationToken cancellationToken)
        {
            using var sc = _serviceScope.CreateScope();
            var gameParents = sc.ServiceProvider.GetRequiredService<Core.Services.IGameParents>();
            var dbContext = sc.ServiceProvider.GetRequiredService<DBContext>();
            var patchNotes = sc.ServiceProvider.GetRequiredService<IPatchNotes>();

            while (!cancellationToken.IsCancellationRequested)
            {
                var stopWatch = Stopwatch.StartNew();

                var parents = await gameParents.All();

                var dataItems = new List<PatchNote>();
                foreach (var language in new[] { "en-us", "fr-fr", "ko-kr", "es-es" })
                {
                    foreach (var parent in parents.Where(x => x.PatchNoteAreas?.Count > 0))
                    {
                        var data = parent.PatchNoteTool switch
                        {
                            "overwatch" => await OverwatchPatchNotes(parent, patchNotes, language),
                            "legacy" => await LegacyPatchNotes(parent, patchNotes, language),
                            _ => null
                        };

                        if(data != null)
                            dataItems.AddRange(data);
                    }
                }

                var hasChanges = false;
                foreach (var item in dataItems)
                {
                    var exist = await dbContext.PatchNotes.FirstOrDefaultAsync(x => 
                        x.Code == item.Code && 
                        x.Type == item.Type && 
                        x.Created == item.Created && 
                        item.Language == x.Language,
                    cancellationToken);

                    if (exist != null)
                    {
                        if (exist.Updated != item.Updated)
                        {
                            exist.Updated = item.Updated;
                            exist.Body = item.Body;

                            dbContext.Update(exist);
                            hasChanges = true;
                        }
                    }
                    else
                    {
                        dbContext.Add(item);
                        hasChanges = true;
                        
                        if (item.Language == "en-us")
                        {
                            await _redisDatabase.PublishAsync("event_notifications", new Notification
                            {
                                NotificationType = NotificationType.PatchNotes,
                                Payload = new Dictionary<string, object>
                                {
                                    { "code", item.Code },
                                    { "flags", item.Type },
                                    { "index_time", item.Created.Ticks }
                                }
                            });
                        }
                    }
                }

                if (hasChanges)
                {
                    await dbContext.SaveChangesAsync(cancellationToken);
                }

                var ts = stopWatch.Elapsed;
                _logger.LogDebug($"Patch Notes took {ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}");

                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
            }
        }

        private static async Task<List<PatchNote>> OverwatchPatchNotes(GameParents parent, IPatchNotes patchNotes, string locale = "en-us")
        {
            var res = new List<PatchNote>();
            var headers = new Dictionary<string, string>
            {
                { "api_key", "blt43efdd4acc4bdcb2" },
                { "access_token", "cs10ce60130ad4ae4fcacf3344" }
            };
            
            foreach (var code in parent.PatchNoteAreas)
            {
                var url =
                    $"https://cdn.blz-contentstack.com/v3/content_types/game_update/entries?desc=post_date&environment=prod&query={{\"type\":\"{code}\", \"expired\":false}}&limit=1&locale={locale}";

                var (parsed, _) = await patchNotes.Get<Overwatch.Root>(url, headers);

                if (parsed.Entries.Count <= 0) continue;
                var f = parsed.Entries.First();

                var note = PatchNote.Create(JsonConvert.SerializeObject(f));
                note.Created = f.PostDate;
                note.Updated = f.UpdatedAt;

                note.Type = code;
                note.Code = parent.Code;
                note.Language = locale;

                res.Add(note);
            }

            return res;
        }

        private static async Task<List<PatchNote>> LegacyPatchNotes(GameParents parent, IPatchNotes patchNotes, string locale = "en-us")
        {
            var res = new List<PatchNote>();

            foreach (var code in parent.PatchNoteAreas)
            {
                var url = $"https://cache-cms-ext-us.battle.net/system/cms/oauth/api/patchnote/list?program={(parent.PatchNoteCode ?? parent.Code)}&region=us&locale={locale}&type={code}&page=1&pageSize=1&orderBy=buildNumber";
                
                var (parsed, _) = await patchNotes.Get<Legacy.Root>(url);
                
                if (parsed.PatchNotes == null) continue;

                var f = parsed.PatchNotes.First();

                var note = PatchNote.Create(JsonConvert.SerializeObject(f));
                note.Created = DateTimeOffset.FromUnixTimeMilliseconds(f.Publish).UtcDateTime;
                if (f.Updated > 0)
                {
                    note.Updated = DateTimeOffset.FromUnixTimeMilliseconds(f.Updated).UtcDateTime;
                }

                note.Type = code;
                note.Code = parent.Code;
                note.Language = locale;

                res.Add(note);
            }

            return res;
        }
    }
}