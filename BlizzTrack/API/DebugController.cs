using Core.Services;
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

        #endregion /game/:code/versions

        #region /starts-with/:code

        [HttpGet("starts-with/{code}")]
        public async Task<IActionResult> StartsWith([FromServices] Core.Models.DBContext dbContext, string code)
        {
            var data = await dbContext.GameConfigs.Where(x => code.ToLower().StartsWith(x.Code)).ToListAsync();

            return Ok(data);
        }

        #endregion /starts-with/:code

        #region /patch-notes/:code/:type

        [HttpGet("patch-notes/{code}/{type}")]
        public async Task<IActionResult> PatchNotes([FromServices] Services.IPatchnotes patchnotes, string code, string type)
        {
            return Ok(await patchnotes.Get(code, type, null));
        }

        #endregion /patch-notes/:code/:type

#endif
    }
}