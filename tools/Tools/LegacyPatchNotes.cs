using Core.Models;
using Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ShellProgressBar;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BNetLib.PatchNotes.Models;
using Tooling.Attributes;

namespace Tooling.Tools
{
    [Tool(Name = "Legacy Patch Notes", Disabled = true)]
    public class LegacyPatchNotes : ITool
    {
        private readonly IGameParents _gameParents;
        private readonly ILogger<LegacyPatchNotes> _logger;
        private readonly DBContext _dbContext;

        public LegacyPatchNotes(IGameParents gameParents, ILogger<LegacyPatchNotes> logger, DBContext dbContext)
        {
            _gameParents = gameParents;
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task Start()
        {
            var stopWatch = Stopwatch.StartNew();

            var games = await _gameParents.All();
            games = games.Where(x => x.PatchNoteAreas?.Count > 0).ToList();

            var dataItems = new List<PatchNote>();

            foreach (var language in new[] { "en-us", "fr-fr", "ko-kr", "es-es" })
            {
                foreach (var parent in games)
                {
                    var data = parent.PatchNoteTool switch
                    {
                        "overwatch" => await OverwatchPatchNotes(parent, language),
                        "legacy" => await OldPatchNotes(parent, language),
                        _ => null
                    };

                    if (parent.PatchNoteTool == "overwatch")
                    {
                        dataItems.AddRange(await OldPatchNotes(parent, language));
                    }

                    dataItems.AddRange(data!);
                }
            }

            var hasChanges = false;
            Console.Clear();

            using var pbar = new ProgressBar(dataItems.Count, "bar", new ProgressBarOptions
            {
                ForegroundColor = ConsoleColor.Yellow,
                ForegroundColorDone = ConsoleColor.DarkGreen,
                BackgroundColor = ConsoleColor.DarkGray,
                BackgroundCharacter = '\u2593'
            });

            var childOptions = new ProgressBarOptions
            {
                ForegroundColor = ConsoleColor.Green,
                BackgroundColor = ConsoleColor.DarkGreen,
                CollapseWhenFinished = true,
                BackgroundCharacter = '\u2593'
            };

            foreach (var item in dataItems)
            {
                pbar.Message = $"Processing: {item.Code} {item.Type} {item.Language}";

                // logger.LogInformation($"Processing: {item.Code} {item.Type} {item.Created}");

                var exist = await _dbContext.PatchNotes.FirstOrDefaultAsync(x => x.Code == item.Code && x.Type == item.Type && x.Created == item.Created && x.Language == item.Language);
                if (exist != null)
                {
                    if (exist.Updated != item.Updated)
                    {
                        exist.Updated = item.Updated;
                        exist.Body = item.Body;
                        exist.Language = item.Language;
                        _dbContext.Update(exist);
                        hasChanges = true;
                    }
                }
                else
                {
                    _dbContext.Add(item);
                    hasChanges = true;
                }

                pbar.Tick();
            }

            if (hasChanges)
            {
                await _dbContext.SaveChangesAsync();
            }

            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
            _logger.LogInformation($"Patch Notes took {elapsedTime}");
        }

        private static async Task<List<PatchNote>> OverwatchPatchNotes(Core.Models.GameParents parent, string locale)
        {
            var res = new List<PatchNote>();

            foreach (var code in parent.PatchNoteAreas)
            {
                var url = $"https://cdn.blz-contentstack.com/v3/content_types/game_update/entries?desc=post_date&environment=prod&query={{\"type\":\"{code}\", \"expired\":false}}&limit=200&locale={locale}";

                var headers = new Dictionary<string, string>()
                {
                    { "api_key", "blt43efdd4acc4bdcb2" },
                    { "access_token", "cs10ce60130ad4ae4fcacf3344" }
                };

                var data = await BNetLib.Http.RemoteJson.Get<Overwatch.Root>(url, headers);

                if (data.Item1.Entries.Count > 0)
                {
                    foreach (var f in data.Item1.Entries) {
                        var note = PatchNote.Create(JsonConvert.SerializeObject(f));
                        note.Created = f.PostDate;
                        note.Updated = f.UpdatedAt;

                        note.Type = code;
                        note.Code = parent.Code;
                        note.Language = locale;

                        res.Add(note);
                    }
                }
            }

            return res;
        }

        private static async Task<List<PatchNote>> OldPatchNotes(Core.Models.GameParents parent, string locale)
        {
            var res = new List<PatchNote>();

            foreach (var code in parent.PatchNoteAreas)
            {
                var c = code;
                if(parent.PatchNoteTool == "overwatch")
                {
                    if (c == "live") c = "retail";
                    if (c == "experimental") continue;
                }

                var url = $"https://cache-cms-ext-us.battle.net/system/cms/oauth/api/patchnote/list?program={(parent.PatchNoteCode ?? parent.Code)}&region=us&locale={locale}&type={c}&page=1&pageSize=200&orderBy=buildNumber";

                Console.WriteLine(url);
                
                var (item1, item2) = await BNetLib.Http.RemoteJson.Get<Legacy.Root>(url);
            
                if(item1.PatchNotes == null || item2 == null)
                {
                    continue;
                }

                foreach (var item in item1.PatchNotes) {
                    var note = PatchNote.Create(JsonConvert.SerializeObject(item));
                    note.Created = DateTimeOffset.FromUnixTimeMilliseconds(item.Publish).UtcDateTime;
                    if (item.Updated > 0)
                    {
                        note.Updated = DateTimeOffset.FromUnixTimeMilliseconds(item.Updated).UtcDateTime;
                    }

                    note.Type = code;
                    note.Code = parent.Code;
                    note.Language = locale;

                    res.Add(note);
                }
            }

            return res;
        }
    }
}
