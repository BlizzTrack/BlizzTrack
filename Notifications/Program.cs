using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Channels;
using System.Threading.Tasks;
using Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Notifications.services;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Newtonsoft;
using Tweetinvi;

namespace Notifications
{
    class Program
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

                    services.AddScoped<Core.Services.ISummary, Core.Services.Summary>();
                    services.AddScoped<Core.Services.IVersions, Core.Services.Versions>();
                    services.AddScoped<Core.Services.ICDNs, Core.Services.CDNs>();
                    services.AddScoped<Core.Services.IBGDL, Core.Services.BGDL>();
                    services.AddScoped<Core.Services.IGameParents, Core.Services.GameParents>();
                    services.AddScoped<Core.Services.IGameChildren, Core.Services.GameChildren>();
                    services.AddScoped<Core.Services.IVersions, Core.Services.Versions>();
                    services.AddSingleton(x => new TwitterClient(
                        hostContext.Configuration.GetValue<string>("Twitter:ConsumerKey"),
                        hostContext.Configuration.GetValue<string>("Twitter:ConsumerKeySecret"),
                        hostContext.Configuration.GetValue<string>("Twitter:AccessToken"),
                        hostContext.Configuration.GetValue<string>("Twitter:AccessTokenSecret")
                        )
                    );

                    var channel = Channel.CreateUnbounded<Notification>();
                    services.AddSingleton(x => channel.Writer);
                    services.AddSingleton(x => channel.Reader);
                    
                    services.AddSingleton<Twitter>();
                    services.AddHostedService<Notify>();
                    services.AddHostedService<Sender>();
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