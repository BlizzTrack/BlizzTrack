using BattleNet.Tools.Services;
using Core.Tooling;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;


namespace BattleNet.Tools
{
    public class Startup : IGameToolStartup
    {
        public void ConfigureServices(ref IServiceCollection services)
        {
            services.AddSingleton<IContentUIService, ContentUIService>();
        }

        public void MapEndpoints(IEndpointRouteBuilder endpoints)
        {
        }
    }
}
