﻿using Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlizzTrack.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class DebugController : ControllerBase
    {
#if DEBUG
        private readonly ILogger<DebugController> _logger;

        public DebugController(ILogger<DebugController> logger)
        {
            _logger = logger;
        }

        #region /game/:code/versions
        [HttpGet("game/{code}/versions")]
        public async Task<IActionResult> Versions([FromServices] IVersions versionService, string code)
        {
            var his = await versionService.Take(code, 2);
            var latest = his.First();
            var previous = his.Last();

            return Ok(new
            {
                latest = new
                {
                    latest.Indexed,
                    latest.Seqn,
                    command = new BNetLib.Networking.Commands.VersionCommand(code).ToString(),
                    Content = latest.Content.Select(x => new
                    {
                        region = x.GetName(),
                        x.Versionsname,
                        x.Buildid,
                        x.Buildconfig,
                        x.Productconfig,
                        x.Cdnconfig
                    }).ToList(),
                },
                previous = new
                {
                    previous.Indexed,
                    previous.Seqn,
                    command = new BNetLib.Networking.Commands.VersionCommand(code).ToString(),
                    Content = previous.Content.Select(x => new
                    {
                        region = x.GetName(),
                        x.Versionsname,
                        x.Buildid,
                        x.Buildconfig,
                        x.Productconfig,
                        x.Cdnconfig
                    }).ToList(),
                }
            });
        }
        #endregion

        #region /starts-with/:code
        [HttpGet("starts-with/{code}")]
        public async Task<IActionResult> StartsWith([FromServices] Core.Models.DBContext dbContext, string code)
        {
            var data = await dbContext.GameConfigs.Where(x => code.ToLower().StartsWith(x.Code)).ToListAsync();

            return Ok(data);
        }
        #endregion

        #region /patch-notes/:code/:type
        public class Note
        {
            public List<BNetLib.Models.Patchnotes.Overwatch.GenericUpdate> GenericUpdates { get; } = new List<BNetLib.Models.Patchnotes.Overwatch.GenericUpdate>();
            public List<BNetLib.Models.Patchnotes.Overwatch.HeroUpdate> HeroUpdates { get; } = new List<BNetLib.Models.Patchnotes.Overwatch.HeroUpdate>();
        }


        [HttpGet("patch-notes/{code}/{type}")]
        public async Task<IActionResult> PatchNotes([FromServices] Core.Models.DBContext dbContext, string code, string type)
        {
            var data = await dbContext.PatchNotes.Where(x => code.ToLower().StartsWith(x.Code) && type.ToLower().Equals(x.Type)).OrderByDescending(x => x.Created).ThenByDescending(x => x.Updated).FirstOrDefaultAsync();
            if (data == null) return NotFound(new { error = "Not Found" });

            if(data.Body.RootElement.TryGetProperty("detail", out var detail))
            {
                return Ok(new
                {
                    detail = detail.GetString()
                });
            }

            var array = data.Body.RootElement.GetProperty("sections").EnumerateArray();

            var note = new Note();

            foreach(var item in array)
            {
                if(item.TryGetProperty("generic_update", out var genericUpdate))
                {
                    if (genericUpdate.ValueKind != System.Text.Json.JsonValueKind.Null)
                    {
                        var update = new BNetLib.Models.Patchnotes.Overwatch.GenericUpdate
                        {
                            Title = genericUpdate.GetProperty("title").GetString(),
                            Description = genericUpdate.GetProperty("description").GetString(),
                            DevComment = genericUpdate.GetProperty("dev_comment").GetString()
                        };

                        var updateItems = genericUpdate.GetProperty("updates").EnumerateArray();
                        update.Updates = new List<BNetLib.Models.Patchnotes.Overwatch.Update>(updateItems.Count());

                        foreach (var updateItem in updateItems.Select(x => x.GetProperty("update")))
                        {
                            var u = new BNetLib.Models.Patchnotes.Overwatch.UpdateChanges
                            {
                                Title = updateItem.GetProperty("title").GetString(),
                                Description = updateItem.GetProperty("description").GetString(),
                                DevComment = updateItem.GetProperty("dev_comment").GetString(),
                                DisplayType = updateItem.GetProperty("display_type").GetString()
                            };

                            update.Updates.Add(new BNetLib.Models.Patchnotes.Overwatch.Update() { UpdateChanges = u });
                        }

                        note.GenericUpdates.Add(update);
                        continue;
                    }
                }

                if (item.TryGetProperty("hero_update", out var heroUpdate))
                {
                    if (heroUpdate.ValueKind != System.Text.Json.JsonValueKind.Null)
                    {

                        var update = new BNetLib.Models.Patchnotes.Overwatch.HeroUpdate
                        {
                            Title = heroUpdate.GetProperty("title").GetString(),
                            Description = heroUpdate.GetProperty("description").GetString(),
                            DevComment = heroUpdate.GetProperty("dev_comment").GetString()
                        };

                        var updateItems = heroUpdate.GetProperty("heroes").EnumerateArray();
                        update.Heroes = new List<BNetLib.Models.Patchnotes.Overwatch.Hero>(updateItems.Count());

                        foreach (var updateItem in updateItems.Select(x => x.GetProperty("hero")))
                        {
                            var u = new BNetLib.Models.Patchnotes.Overwatch.HeroChanges
                            {
                                HeroName = updateItem.GetProperty("hero_name").GetString(),
                                ChangeDescription = updateItem.GetProperty("change_description").GetString(),
                                DevComment = updateItem.GetProperty("dev_comment").GetString()
                            };

                            var abilities = updateItem.GetProperty("abilities").EnumerateArray();
                            u.Abilities = new List<BNetLib.Models.Patchnotes.Overwatch.Ability>(abilities.Count());

                            foreach (var ability in abilities.Select(x => x.GetProperty("ability")))
                            {
                                var a = new BNetLib.Models.Patchnotes.Overwatch.AbilityChanges
                                {
                                    AbilityName = ability.GetProperty("ability_name").GetString(),
                                    ChangeDescription = ability.GetProperty("change_description").GetString(),
                                    DevComment = ability.GetProperty("dev_comment").GetString()
                                };

                                u.Abilities.Add(new BNetLib.Models.Patchnotes.Overwatch.Ability() { AbilityChanges = a });
                            }

                            update.Heroes.Add(new BNetLib.Models.Patchnotes.Overwatch.Hero() { HeroChanges = u });
                        }

                        note.HeroUpdates.Add(update);

                        continue;
                    }
                }
            }

            
            return Ok(note);
        }
        #endregion

#endif
    }
}
