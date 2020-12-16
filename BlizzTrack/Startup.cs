using Core.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Newtonsoft;

namespace BlizzTrack
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddResponseCompression();

            services.Configure<RazorViewEngineOptions>(o =>
            {
                o.ViewLocationFormats.Add
                    ("/Pages/Partials/{0}" + RazorViewEngine.ViewExtension);
                o.PageViewLocationFormats.Add("/Pages/Partials/{0}" + RazorViewEngine.ViewExtension);
            });

            services.AddControllers().AddNewtonsoftJson(options => options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented);
            services.AddControllersWithViews().AddNewtonsoftJson(options => options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented);
            services.AddRazorPages().AddNewtonsoftJson(options => options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented);
            services.Configure<MvcNewtonsoftJsonOptions>(x => x.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore);

            services.AddDbContextPool<DBContext>(options =>
                options.UseNpgsql(
                    Configuration.GetConnectionString("ConnectionString"), o =>
                    {
                        o.UseTrigrams();
                    }).EnableSensitiveDataLogging());

            services.AddMvc(options => options.EnableEndpointRouting = false)
                .SetCompatibilityVersion(CompatibilityVersion.Latest).AddNewtonsoftJson();

            var conf = new RedisConfiguration()
            {
                AbortOnConnectFail = false,
                KeyPrefix = "BD_",
                Hosts = new RedisHost[]
                {
                    new RedisHost { Host = Configuration.GetConnectionString("redis") ?? "localhost", Port = 6379 }
                },
                AllowAdmin = true,
                ConnectTimeout = 1000,
                PoolSize = 20,
                Database = 0,
                ServerEnumerationStrategy = new ServerEnumerationStrategy()
                {
                    Mode = ServerEnumerationStrategy.ModeOptions.All,
                    TargetRole = ServerEnumerationStrategy.TargetRoleOptions.Any,
                    UnreachableServerAction = ServerEnumerationStrategy.UnreachableServerActionOptions.Throw
                }
            };

            services.AddStackExchangeRedisExtensions<NewtonsoftSerializer>(conf);

            // Blizzard services
            services.AddSingleton(x => new BNetLib.Networking.BNetClient());
            services.AddSingleton(x => new BNetLib.Http.ProductConfig());

            // System Services
            services.AddSingleton<Services.IBlizzardAlerts, Services.BlizzardAlerts>();

            // Shared services
            services.AddScoped<Core.Services.ISummary, Core.Services.Summary>();
            services.AddScoped<Core.Services.IVersions, Core.Services.Versions>();
            services.AddScoped<Core.Services.ICDNs, Core.Services.CDNs>();
            services.AddScoped<Core.Services.IBGDL, Core.Services.BGDL>();
            services.AddScoped<Core.Services.IGameConfig, Core.Services.GameConfig>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            InitializeDatabase(app);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseResponseCompression();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseMvcWithDefaultRoute();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }

        private void InitializeDatabase(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope();

            var ctx = scope.ServiceProvider.GetRequiredService<DBContext>();
            ctx.Database.Migrate();
        }
    }
}
