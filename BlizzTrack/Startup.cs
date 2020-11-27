using Core.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

            services.AddControllersWithViews();

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });

#if DEBUG
            services.AddControllers().AddNewtonsoftJson(options => options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented);
#else 
            services.AddControllers().AddNewtonsoftJson();
#endif

            services.Configure<MvcNewtonsoftJsonOptions>(x => x.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore);

            services.AddDbContext<DBContext>(options =>
                options.UseNpgsql(
                    Configuration.GetConnectionString("ConnectionString"), o =>
                    {
                        o.UseTrigrams();
                    }).EnableSensitiveDataLogging());

            services.AddMvc(options => options.EnableEndpointRouting = false)
                .SetCompatibilityVersion(CompatibilityVersion.Latest).AddNewtonsoftJson();

            services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull);
            services.AddSingleton(x => new BNetLib.Networking.BNetClient());
            services.AddSingleton(x => new BNetLib.Http.ProductConfig());

            services.AddScoped<Core.Services.ISummary, Core.Services.Summary>();
            services.AddScoped<Core.Services.IVersions, Core.Services.Versions>();
            services.AddScoped<Core.Services.ICDNs, Core.Services.CDNs>();
            services.AddScoped<Core.Services.IBGDL, Core.Services.BGDL>();
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

            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
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
