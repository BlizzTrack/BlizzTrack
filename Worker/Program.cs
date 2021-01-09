using Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace Worker
{
    internal class Program
    {
        public static async Task Main(string[] args) => await CreateHostBuilder(args).RunAsync();

        public static IHost CreateHostBuilder(string[] args)
        {
            var app = new HostBuilder()
                .ConfigureHostConfiguration(configHost => configHost.AddEnvironmentVariables())
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory())
                        .AddEnvironmentVariables()
                        .AddJsonFile("appsettings.json", true)
                        .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", true);

                    if (args != null) config.AddCommandLine(args);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddDbContext<DBContext>(options =>
                        options.UseNpgsql(
                            hostContext.Configuration.GetConnectionString("ConnectionString"), o =>
                            {
                                o.UseTrigrams();
                            }));

                    services.AddSingleton(x => new BNetLib.Networking.BNetClient(BNetLib.Networking.ServerRegion.US));
                    services.AddSingleton<BNetLib.Http.ProductConfig>();

                    services.AddScoped<Core.Services.ISummary, Core.Services.Summary>();
                    services.AddScoped<Core.Services.IVersions, Core.Services.Versions>();
                    services.AddScoped<Core.Services.ICDNs, Core.Services.CDNs>();
                    services.AddScoped<Core.Services.IBGDL, Core.Services.BGDL>();
                    services.AddScoped<Core.Services.IGameParents, Core.Services.GameParents>();

                    services.AddHostedService<Workers.PatchnotesHosted>();
                    services.AddHostedService<Workers.SummaryHosted>();

                    services.AddScoped<Core.Services.IVersions, Core.Services.Versions>();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                }).Build();

            InitializeDatabase(app);

            return app;
        }

        private static void InitializeDatabase(IHost app)
        {
            var ctx = app.Services.GetRequiredService<DBContext>();
            ctx.Database.Migrate();
        }
    }
}