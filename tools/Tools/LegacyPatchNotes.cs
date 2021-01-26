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
using Tooling.Attributes;

namespace Tooling.Tools
{
    [Tool(Name = "Legacy Patch Notes", Disabled = false)]
    public class LegacyPatchNotes : ITool
    {
        private readonly IGameParents gameParents;
        private readonly ILogger<LegacyPatchNotes> logger;
        private readonly DBContext dbContext;

        public LegacyPatchNotes(IGameParents gameParents, ILogger<LegacyPatchNotes> logger, DBContext dbContext)
        {
            this.gameParents = gameParents;
            this.logger = logger;
            this.dbContext = dbContext;
        }

        public async Task Start()
        {
            var stopWatch = Stopwatch.StartNew();

            var games = await gameParents.All();
            games = games.Where(x => x.PatchNoteAreas?.Count > 0).ToList();

            var dataItems = new List<PatchNote>();

            foreach (var parent in games)
            {
                List<PatchNote> data = parent.PatchNoteTool switch
                {
                    "overwatch" => await OverwatchPatchNotes(parent),
                    "legacy" => await OldPatchNotes(parent),
                    _ => null
                };

                if (parent.PatchNoteTool == "overwatch")
                {
                    dataItems.AddRange(await OldPatchNotes(parent));
                }

                dataItems.AddRange(data);

                // logger.LogInformation($"{parent.Name} has size: {data.Count}");
            }

            // logger.LogInformation($"Total size has size: {dataItems.Count}");

            var hasChanges = false;
            Console.Clear();

            using var pbar = new ProgressBar(dataItems.Count(), "bar", new ProgressBarOptions
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
                pbar.Message = $"Processing: {item.Code} {item.Type}";

                // logger.LogInformation($"Processing: {item.Code} {item.Type} {item.Created}");

                var exist = await dbContext.PatchNotes.FirstOrDefaultAsync(x => x.Code == item.Code && x.Type == item.Type && x.Created == item.Created);
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
                }

                pbar.Tick();
            }

            if (hasChanges)
            {
                await dbContext.SaveChangesAsync();
            }

            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
            // logger.LogInformation($"Patch Notes took {elapsedTime}");
        }

        private static async Task<List<PatchNote>> OverwatchPatchNotes(Core.Models.GameParents parent)
        {
            var res = new List<PatchNote>();

            foreach (var code in parent.PatchNoteAreas)
            {
                var url = $"https://cdn.blz-contentstack.com/v3/content_types/game_update/entries?desc=post_date&environment=prod&query={{\"type\":\"{code}\", \"expired\":false}}&limit=200&locale=en-us";

                var headers = new Dictionary<string, string>()
                {
                    { "api_key", "blt43efdd4acc4bdcb2" },
                    { "access_token", "cs10ce60130ad4ae4fcacf3344" }
                };

                var data = await BNetLib.Http.RemoteJson.Get<BNetLib.Models.Patchnotes.Overwatch.Root>(url, headers);

                if (data.Item1.Entries.Count > 0)
                {
                    foreach (var f in data.Item1.Entries) {
                        var note = PatchNote.Create(JsonConvert.SerializeObject(f));
                        note.Created = f.PostDate;
                        note.Updated = f.UpdatedAt;

                        note.Type = code;
                        note.Code = parent.Code;

                        res.Add(note);
                    }
                }
            }

            return res;
        }

        private static async Task<List<PatchNote>> OldPatchNotes(Core.Models.GameParents parent)
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

                var url = $"https://cache-cms-ext-us.battle.net/system/cms/oauth/api/patchnote/list?program={(parent.PatchNoteCode ?? parent.Code)}&region=us&locale=enUS&type={c}&page=1&pageSize=200&orderBy=buildNumber";

                var data = await BNetLib.Http.RemoteJson.Get<BNetLib.Models.Patchnotes.Legacy.Root>(url);
    
                foreach (var item in data.Item1.PatchNotes) {
                    var f = item;

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
            }

            return res;
        }
    }
}
