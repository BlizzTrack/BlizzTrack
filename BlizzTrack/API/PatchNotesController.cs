using Core.Models;
using Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlizzTrack.API
{
    [Route("api/patch-notes"), ApiController, ApiExplorerSettings(GroupName = "Game Patch Notes"),
     Produces("application/json")]
    public class PatchNotesController : ControllerBase
    {
        private readonly IGameParents _gameParents;
        private readonly Services.IPatchNotes _patchNotes;

        public PatchNotesController(IGameParents gameParents, Services.IPatchNotes patchNotes)
        {
            _gameParents = gameParents;
            _patchNotes = patchNotes;
        }

        /// <summary>
        ///     List all games with supported patch notes
        /// </summary>
        /// <returns>List all games with supported patch notes</returns>
        /// <response code="200">Returns list of all games with relations to there patch notes</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PatchNoteResults.PatchNoteRef))]
        public async Task<IActionResult> List()
        {
            var parents = await _gameParents.All();
            parents = parents.Where(x => x.PatchNoteAreas?.Count > 0).ToList();

            var returnResults = new List<PatchNoteResults.PatchNoteRef>();

            foreach(var parent in parents)
            {
                var items = new Dictionary<string, string>();

                for(var i = 0; i < parent.PatchNoteAreas.Count; i++)
                {
                    items[parent.PatchNoteAreas[i]] = Url.Action("view", "PatchNotes", new { game = parent.Slug, game_type = parent.PatchNoteAreas[i] }, Scheme());
                }

                items["all"] = Url.Action("List", "PatchNotes", new { game = parent.Slug }, Scheme());

                returnResults.Add(new PatchNoteResults.PatchNoteRef
                {
                    Name = parent.Name,
                    Code = parent.Code,
                    Logos = parent.Logos,
                    Types = items,
                });
            }

            return Ok(returnResults);
        }

        /// <summary>
        ///     All latest patch notes for given game
        /// </summary>
        /// <returns>Latest patch notes for given game</returns>
        /// <response code="200">Latest patch notes for given game</response>
        /// <param name="game">The game slug (EX: overwatch)</param>
        [HttpGet("{game}"),
         ProducesResponseType(StatusCodes.Status200OK,
             Type = typeof(PatchNoteResults.Result<Dictionary<string, PatchNoteResults.PatchNoteBody>>)),
         ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ReponseTypes.NotFound))]
        public async Task<IActionResult> List(string game)
        {
            var parent = await _gameParents.Get(game);
            if (parent == null) return NotFound(new ReponseTypes.NotFound());

            var items = new Dictionary<string, PatchNoteResults.PatchNoteBody>();

            foreach (var code in parent.PatchNoteAreas)
            {
                var data = await _patchNotes.Get(parent.Code, code);

                PatchNoteResults.PatchNoteBodyPayload payload;
                if (!string.IsNullOrEmpty(data.Details))
                {
                    payload = new PatchNoteResults.PatchNoteBodyPayload  { Details = data.Details };
                }
                else
                {
                    payload = new PatchNoteResults.PatchNoteBodyPayload
                    {
                        GenericUpdates = data.GenericUpdates,
                        HeroUpdates = data.HeroUpdates
                    };
                }

                var types = new Dictionary<string, string>();

                for (var i = 0; i < parent.PatchNoteAreas.Count; i++)
                {
                    types[parent.PatchNoteAreas[i]] = Url.Action("view", "PatchNotes", new { game = parent.Slug, game_type = parent.PatchNoteAreas[i] }, Scheme());
                }

                types["all"] = Url.Action("List", "PatchNotes", new { game = parent.Slug }, Scheme());

                items[code] = new PatchNoteResults.PatchNoteBody
                {
                    Created = data.Created,
                    Updated = data.Updated,
                    Mode = parent.PatchNoteTool,
                    Types = types,
                    Payload = payload
                };
            }

            var r = new PatchNoteResults.Result<Dictionary<string, PatchNoteResults.PatchNoteBody>>
            {
                Name = parent.Name,
                Code = parent.Code,
                Logos = parent.Logos,
                Results = items
            };

            return Ok(r);
        }

        /// <summary>
        ///     Latest patch note for game and game_type
        /// </summary>
        /// <returns>Latest patch note for game and game_type</returns>
        /// <response code="200">Returns latest patch notes for given game_type</response>
        /// <param name="game">The game slug (EX: overwatch)</param>
        /// <param name="gameType">The game type (EX: ptr)</param>
        [HttpGet("{game}/{game_type}"),
         ProducesResponseType(StatusCodes.Status200OK,
             Type = typeof(PatchNoteResults.Result<Dictionary<string, PatchNoteResults.PatchNoteBody>>)),
         ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ReponseTypes.NotFound))]
        public async Task<IActionResult> View(string game, string gameType)
        {
            var parent = await _gameParents.Get(game);
            if (parent == null) return NotFound(new ReponseTypes.NotFound());

            if (!parent.PatchNoteAreas.Contains(gameType.ToLower())) return NotFound(new ReponseTypes.NotFound());

            var data = await _patchNotes.Get(parent.Code, gameType);

            PatchNoteResults.PatchNoteBodyPayload payload;
            if (!string.IsNullOrEmpty(data.Details))
            {
                payload = new PatchNoteResults.PatchNoteBodyPayload { Details = data.Details };
            }
            else
            {
                payload = new PatchNoteResults.PatchNoteBodyPayload
                {
                    GenericUpdates = data.GenericUpdates,
                    HeroUpdates = data.HeroUpdates
                };
            }

            var types = new Dictionary<string, string>();

            foreach (var t in parent.PatchNoteAreas)
            {
                types[t] = Url.Action("view", "PatchNotes", new { game = parent.Slug, game_type = t }, Scheme());
            }

            types["all"] = Url.Action("List", "PatchNotes", new { game = parent.Slug }, Scheme());

            var items = new PatchNoteResults.PatchNoteBody()
            {
                Created = data.Created,
                Updated = data.Updated,
                Mode = parent.PatchNoteTool,
                Types = types,
                Payload = payload
            };

            return Ok(new PatchNoteResults.Result<Dictionary<string, PatchNoteResults.PatchNoteBody>>
            {
                Name = parent.Name,
                Code = parent.Code,
                Logos = parent.Logos,
                Results = new Dictionary<string, PatchNoteResults.PatchNoteBody>
                    {
                        { gameType.ToLower(), items }
                    },
            });
        }

        private string Scheme()
        {
            if (HttpContext.Request.Host.Host.Contains("blizztrack", StringComparison.OrdinalIgnoreCase))
                return "https";

            return "http";
        }
    }

    public class PatchNoteResults
    {
        public class Result<T>
        {
            /// <summary>
            ///     Game Name
            /// </summary>

            public string Name { get; set; }

            /// <summary>
            ///     Game Code/Slug
            /// </summary>

            public string Code { get; set; }

            /// <summary>
            ///     Logos we stored about the game
            /// </summary>
            public List<Icons> Logos { get; set; }


            public T Results { get; set; }
        }

        public class PatchNoteRef
        {
            /// <summary>
            ///     Game Name
            /// </summary>

            public string Name { get; set; }

            /// <summary>
            ///     Game Code/Slug
            /// </summary>

            public string Code { get; set; }

            /// <summary>
            ///     Logos we stored about the game
            /// </summary>
            public List<Icons> Logos { get; set; }

            /// <summary>
            ///     Other types of patch notes for this game (EX: retail, ptr, beta)
            /// </summary>

            public Dictionary<string, string> Types { get; set; }
        }

        public class PatchNoteBody
        {
            /// <summary>
            ///     Time published
            /// </summary>

            public DateTime Created { get; set; }

            /// <summary>
            ///     Time Updated
            /// </summary>
            public DateTime Updated { get; set; }

            /// <summary>
            ///     Name of the patch note type
            /// </summary>

            public string Mode { get; set; }

            /// <summary>
            ///     Other types of patch notes for this game (EX: retail, ptr, beta)
            /// </summary>

            public Dictionary<string, string> Types { get; set; }

            /// <summary>
            ///     Patch notes content
            /// </summary>

            public PatchNoteBodyPayload Payload { get; set; }
        }

        public class PatchNoteBodyPayload
        {
            /// <summary>
            ///     Raw HTML of the patch note (Not used for newer Overwatch notes)
            /// </summary>
            public string Details { get; set; } = null;

            /// <summary>
            ///     Only used if set game is Overwatch
            /// </summary>
            public List<BNetLib.Models.Patchnotes.Overwatch.HeroUpdate> HeroUpdates { get; set; } = null;

            /// <summary>
            ///     Only used if set game is Overwatch
            /// </summary>
            public List<BNetLib.Models.Patchnotes.Overwatch.GenericUpdate> GenericUpdates { get; set; } = null;
        } 
    }
}
