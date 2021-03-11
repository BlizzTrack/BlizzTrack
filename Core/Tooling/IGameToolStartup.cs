using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Tooling
{
    public interface IGameToolStartup
    {
        public void ConfigureServices(ref IServiceCollection services);

        void MapEndpoints(IEndpointRouteBuilder endpoints);
    }
}
