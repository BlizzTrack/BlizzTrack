﻿using Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Tooling.Attributes;
using Tooling.Tools;

namespace tools
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args);

            var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger<Program>();


            var tools = LoadTools(host.Services);

            foreach(var (key, value) in tools)
            {
                logger.LogInformation($"Running: {key}");
                await value.Start();
            }
        }

        private static Dictionary<string, ITool> LoadTools(IServiceProvider services)
        {
            var tasks = Assembly.GetExecutingAssembly().GetTypes().Where(typ => typ.GetInterfaces().Contains(typeof(ITool)) && !typ.IsAbstract && typ.GetCustomAttributes(typeof(ToolAttribute), true).Length > 0).ToList();

            var results = new Dictionary<string, ITool>();
            foreach (var task in tasks)
            {
                if (task.GetCustomAttributes(typeof(ToolAttribute), true).First() is ToolAttribute attr)
                {
                    var name = attr.Name;
                    if (string.IsNullOrEmpty(name)) name = task.Name.ToLower();

                    if (attr.Disabled) continue;

                    var tool = (ITool)ActivatorUtilities.CreateInstance(services, task);
                    results.Add(name, tool);
                }
            }

            return results;
        }

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
