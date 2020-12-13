using Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
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

#endif
    }
}
