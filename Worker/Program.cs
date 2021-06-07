using Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using BNetLib.Ribbit;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Newtonsoft;
using Worker.Events;

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

                    var conf = new RedisConfiguration
                    {
                        ConnectionString = hostContext.Configuration.GetConnectionString("redis"),
                        AllowAdmin = true,
                        Database = 2,
                    };
                    services.AddStackExchangeRedisExtensions<NewtonsoftSerializer>(conf);

                    services.AddSingleton(_ => new BNetClient());
                    services.AddSingleton<BNetLib.Http.ProductConfig>();

                    services.AddScoped<Core.Services.ISummary, Core.Services.Summary>();
                    services.AddScoped<Core.Services.IVersions, Core.Services.Versions>();
                    services.AddScoped<Core.Services.ICDNs, Core.Services.CDNs>();
                    services.AddScoped<Core.Services.IBGDL, Core.Services.BGDL>();
                    services.AddScoped<Core.Services.IGameParents, Core.Services.GameParents>();
                    services.AddScoped<Core.Services.IVersions, Core.Services.Versions>();
                    services.AddScoped<BNetLib.PatchNotes.IPatchNotes, BNetLib.PatchNotes.PatchNotes>();
                    
                    services.AddSingleton(x => new ConcurrentQueue<ConfigUpdate>());

                    services.AddHostedService<Workers.PatchnotesHosted>();
                    services.AddHostedService<Workers.SummaryHosted>();
                    services.AddHostedService<Workers.CatalogWorkerHosted>();

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