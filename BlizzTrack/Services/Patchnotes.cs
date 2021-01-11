using BlizzTrack.Models;
using BNetLib.Models.Patchnotes;
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
        Task<PatchNoteData> Get(string code, string type);

        Task<List<PatchNoteData>> Get(string code, params string[] type);
    }

    public class Patchnotes : IPatchnotes
    {
        private readonly DBContext _dbContext;

        public Patchnotes(DBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PatchNoteData> Get(string code, string type)
        {
            var data = await _dbContext.PatchNotes.Where(x => code.ToLower().StartsWith(x.Code) && type.ToLower().Equals(x.Type)).OrderByDescending(x => x.Created).ThenByDescending(x => x.Updated).FirstOrDefaultAsync();
            if (data == null) return null;

            var note = new PatchNoteData()
            {
                Created = data.Created,
                Updated = data.Updated,
            };


            if (data.Body.RootElement.TryGetProperty("detail", out var detail))
            {
                note.Details = detail.GetString();
                return note;
            }

            var array = data.Body.RootElement.GetProperty("sections").EnumerateArray();

            foreach (var item in array)
            {
                if (item.TryGetProperty("generic_update", out var genericUpdate))
                {
                    if (genericUpdate.ValueKind != System.Text.Json.JsonValueKind.Null)
                    {
                        if (note.GenericUpdates == null) note.GenericUpdates = new List<Overwatch.GenericUpdate>();

                        var update = new Overwatch.GenericUpdate
                        {
                            Title = genericUpdate.GetProperty("title").GetString(),
                            Description = genericUpdate.GetProperty("description").GetString(),
                            DevComment = genericUpdate.GetProperty("dev_comment").GetString()
                        };

                        var updateItems = genericUpdate.GetProperty("updates").EnumerateArray();
                        update.Updates = new List<Overwatch.Update>(updateItems.Count());

                        foreach (var updateItem in updateItems.Select(x => x.GetProperty("update")))
                        {
                            var u = new Overwatch.UpdateChanges
                            {
                                Title = updateItem.GetProperty("title").GetString(),
                                Description = updateItem.GetProperty("description").GetString(),
                                DevComment = updateItem.GetProperty("dev_comment").GetString(),
                                DisplayType = updateItem.GetProperty("display_type").GetString()
                            };

                            update.Updates.Add(new Overwatch.Update() { UpdateChanges = u });
                        }

                        note.GenericUpdates.Add(update);
                    }
                }

                if (item.TryGetProperty("hero_update", out var heroUpdate))
                {
                    if (heroUpdate.ValueKind != System.Text.Json.JsonValueKind.Null)
                    {
                        if (note.HeroUpdates == null) note.HeroUpdates = new List<Overwatch.HeroUpdate>();

                        var update = new Overwatch.HeroUpdate
                        {
                            Title = heroUpdate.GetProperty("title").GetString(),
                            Description = heroUpdate.GetProperty("description").GetString(),
                            DevComment = heroUpdate.GetProperty("dev_comment").GetString(),
                        };

                        var updateItems = heroUpdate.GetProperty("heroes").EnumerateArray();
                        update.Heroes = new List<Overwatch.Hero>(updateItems.Count());

                        foreach (var updateItem in updateItems.Select(x => x.GetProperty("hero")))
                        {
                            var u = new Overwatch.HeroChanges
                            {
                                HeroName = updateItem.GetProperty("hero_name").GetString(),
                                ChangeDescription = updateItem.GetProperty("change_description").GetString(),
                                DevComment = updateItem.GetProperty("dev_comment").GetString(),
                                Metadata = ReadMetaData(updateItem.GetProperty("metadata")),
                            };

                            var abilities = updateItem.GetProperty("abilities").EnumerateArray();
                            u.Abilities = new List<Overwatch.Ability>(abilities.Count());

                            foreach (var ability in abilities.Select(x => x.GetProperty("ability")))
                            {
                                var a = new Overwatch.AbilityChanges
                                {
                                    AbilityName = ability.GetProperty("ability_name").GetString(),
                                    ChangeDescription = ability.GetProperty("change_description").GetString(),
                                    DevComment = ability.GetProperty("dev_comment").GetString(),
                                    Metadata = ReadMetaData(ability.GetProperty("metadata"))
                                };

                                u.Abilities.Add(new Overwatch.Ability() { AbilityChanges = a });
                            }

                            update.Heroes.Add(new Overwatch.Hero() { HeroChanges = u });
                        }

                        note.HeroUpdates.Add(update);
                    }
                }
            }

            return note;
        }

        public async Task<List<PatchNoteData>> Get(string code, params string[] types)
        {
            var items = new List<PatchNoteData>();

            foreach(var type in types)
            {
                var data = await Get(code, type);
                if (data == null) continue;

                items.Add(data);
            }

            return items;
        }

        private static Overwatch.Metadata ReadMetaData(System.Text.Json.JsonElement jsonElement)
        {
            if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.Null) return null;

            return new Overwatch.Metadata()
            {
                IconGuid = jsonElement.GetProperty("icon_guid").GetString(),
                AssetGuid = jsonElement.GetProperty("asset_guid").GetString()
            };
        }
    }
}
