﻿using BlizzTrack.Models;
using Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlizzTrack.Services
{
    public interface IPatchnotes
    {
        Task<PatchnoteData> Get(string code, string type);
    }

    public class Patchnotes : IPatchnotes
    {
        private readonly DBContext _dbContext;

        public Patchnotes(DBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PatchnoteData> Get(string code, string type)
        {
            var data = await _dbContext.PatchNotes.Where(x => code.ToLower().StartsWith(x.Code) && type.ToLower().Equals(x.Type)).OrderByDescending(x => x.Created).ThenByDescending(x => x.Updated).FirstOrDefaultAsync();
            if (data == null) return null;

            if (data.Body.RootElement.TryGetProperty("detail", out var detail))
            {
                return new PatchnoteData()
                {
                    Created = data.Created,
                    Updated = data.Updated,
                    Detailts = detail.GetString()
                };
            }

            var note = new PatchnoteData()
            {
                Created = data.Created,
                Updated = data.Updated,
            };

            var array = data.Body.RootElement.GetProperty("sections").EnumerateArray();

            foreach (var item in array)
            {
                if (item.TryGetProperty("generic_update", out var genericUpdate))
                {
                    if (genericUpdate.ValueKind != System.Text.Json.JsonValueKind.Null)
                    {
                        if (note.GenericUpdates == null) note.GenericUpdates = new List<BNetLib.Models.Patchnotes.Overwatch.GenericUpdate>();

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
                        if (note.HeroUpdates == null) note.HeroUpdates = new List<BNetLib.Models.Patchnotes.Overwatch.HeroUpdate>();

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

            return note;
        }
    }
}