using Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlizzTrack.API
{
    [Route("api/patch-notes")]
    [ApiController]
    public class PatchNotesController : ControllerBase
    {
        private readonly Core.Services.IGameParents gameParents;
        private readonly Services.IPatchnotes patchnotes;

        public PatchNotesController(IGameParents gameParents, Services.IPatchnotes patchnotes)
        {
            this.gameParents = gameParents;
            this.patchnotes = patchnotes;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var scheme = Request.Scheme;
            if (Request.Host.Host.Contains("blizztrack", StringComparison.OrdinalIgnoreCase))
            {
                scheme = "https";
            }

            var parents = await gameParents.All();
            parents = parents.Where(x => x.PatchNoteAreas?.Count > 0).ToList();

            var returnResults = new List<object>();

            foreach(var parent in parents)
            {
                var items = new Dictionary<string, string>();

                for(var i = 0; i < parent.PatchNoteAreas.Count; i++)
                {
                    items[parent.PatchNoteAreas[i]] = Url.Action("view", "PatchNotes", new { game = parent.Slug, game_type = parent.PatchNoteAreas[i] }, scheme);
                }

                items["all"] = Url.Action("view", "PatchNotes", new { game = parent.Slug }, scheme);

                returnResults.Add(new
                {
                    parent.Name,
                    parent.Code,
                    parent.Logos,
                    Types = items,
                });
            }

            return Ok(returnResults);
        }

        [HttpGet("{game}/{game_type?}")]
        public async Task<IActionResult> View(string game, string game_type = default)
        {
            var scheme = Request.Scheme;
            if (Request.Host.Host.Contains("blizztrack", StringComparison.OrdinalIgnoreCase))
            {
                scheme = "https";
            }

            var parent = await gameParents.GetBySlug(game);

            // Support code
            if (parent == null) parent = await gameParents.Get(game);
            if (parent == null) return NotFound();

            if (string.IsNullOrEmpty(game_type) || game_type == default)
            {
                var items = new Dictionary<string, object>();

                foreach (var code in parent.PatchNoteAreas) {
                    var data = await patchnotes.Get(parent.Code, code);

                    object payload;
                    if(!string.IsNullOrEmpty(data.Details))
                    {
                        payload = new { data.Details };
                    } else
                    {
                        payload = new
                        {
                            data.GenericUpdates,
                            data.HeroUpdates
                        };
                    }

                    var types = new Dictionary<string, string>();

                    for (var i = 0; i < parent.PatchNoteAreas.Count; i++)
                    {
                        types[parent.PatchNoteAreas[i]] = Url.Action("view", "PatchNotes", new { game = parent.Slug, game_type = parent.PatchNoteAreas[i] }, scheme);
                    }

                    types["all"] = Url.Action("view", "PatchNotes", new { game = parent.Slug }, scheme);

                    items[code] = new
                    {
                        data.Created,
                        data.Updated,
                        Mode = parent.PatchNoteTool,
                        Types = types,
                        Payload = payload
                    };
                }

                return Ok(new
                {
                    parent.Name,
                    parent.Code,
                    parent.Logos,
                    Results = items,
                });
            }

            if (!parent.PatchNoteAreas.Contains(game_type.ToLower())) return NotFound();

            {
                var data = await patchnotes.Get(parent.Code, game_type);

                object payload;
                if (!string.IsNullOrEmpty(data.Details))
                {
                    payload = new { data.Details };
                }
                else
                {
                    payload = new
                    {
                        data.GenericUpdates,
                        data.HeroUpdates
                    };
                }

                var types = new Dictionary<string, string>();

                for (var i = 0; i < parent.PatchNoteAreas.Count; i++)
                {
                    types[parent.PatchNoteAreas[i]] = Url.Action("view", "PatchNotes", new { game = parent.Slug, game_type = parent.PatchNoteAreas[i] }, scheme);
                }

                types["all"] = Url.Action("view", "PatchNotes", new { game = parent.Slug }, scheme);

                var items = new
                {
                    data.Created,
                    data.Updated,
                    Mode = parent.PatchNoteTool,
                    Types = types,
                    Payload = payload
                };

                return Ok(new
                {
                    parent.Name,
                    parent.Code,
                    parent.Logos,
                    Results = new Dictionary<string, object>
                    {
                        { game_type.ToLower(), items }
                    },
                });
            }
        }
    }
}
