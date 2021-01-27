using Core.Tooling;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Overwatch.Tools
{
    public class Startup : IGameToolStartup
    {
        public void ConfigureServices(ref IServiceCollection services)
        {
            services.AddSingleton<Services.IOverwatchProfileService, Services.OverwatchProfileService>();
        }

        public void MapEndpoints(IEndpointRouteBuilder endpoints)
        {
        }
    }
}
