using Core.Models;
using Metalface.AspNetCore.ServerTiming;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement;
using Microsoft.Net.Http.Headers;
using Minio.AspNetCore;
using Newtonsoft.Json.Converters;

namespace BlizzMeta
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
            services.AddServerTiming();
            
            services.AddFeatureManagement(Configuration.GetSection("Features"));

            services.Configure<RazorViewEngineOptions>(o =>
            {
                o.ViewLocationFormats.Add
                    ("/Pages/Partials/{0}" + RazorViewEngine.ViewExtension);
                o.PageViewLocationFormats.Add("/Pages/Partials/{0}" + RazorViewEngine.ViewExtension);
            });
            
            services.AddControllers()
                .AddNewtonsoftJson(options =>
                    options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented)
                .AddXmlSerializerFormatters();
            services.AddControllersWithViews().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
                options.SerializerSettings.Converters.Add(new StringEnumConverter());
            });
            services.AddRazorPages().AddNewtonsoftJson(options =>
                options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented);
            services.Configure<MvcNewtonsoftJsonOptions>(x =>
                x.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore);
            
            services.AddDbContext<DBContext>(options =>
                options.UseNpgsql(
                        Configuration.GetConnectionString("ConnectionString"), o => { o.UseTrigrams(); })
                    .EnableSensitiveDataLogging());
            
            services.AddDataProtection().PersistKeysToDbContext<DBContext>();
            
            services.AddMvc(options => options.EnableEndpointRouting = false)
                .SetCompatibilityVersion(CompatibilityVersion.Latest).AddNewtonsoftJson();
            
            services.AddMinio(options =>
            {
                options.Endpoint = Configuration.GetValue("AWS:Endpoint", "");
                options.SecretKey = Configuration.GetValue("AWS:SecretKey", "");
                options.AccessKey = Configuration.GetValue("AWS:AccessKey", "");
                options.OnClientConfiguration = client => { client.WithSSL(); };
            });
            
            services.AddScoped<Core.Services.ISummary, Core.Services.Summary>();
            services.AddScoped<Core.Services.IVersions, Core.Services.Versions>();
            services.AddScoped<Core.Services.ICDNs, Core.Services.CDNs>();
            services.AddScoped<Core.Services.IBGDL, Core.Services.BGDL>();
            services.AddScoped<Core.Services.IGameConfig, Core.Services.GameConfig>();
            services.AddScoped<Core.Services.IGameParents, Core.Services.GameParents>();
            services.AddScoped<Core.Services.ICatalog, Core.Services.Catalog>();
            services.AddScoped<Core.Services.IGameCompanies, Core.Services.GameCompanies>();
            services.AddScoped<Core.Services.IGameChildren, Core.Services.GameChildren>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
                context.Response.Headers.Add("X-Xss-Protection", "1");
                await next();
            });
            
            InitializeDatabase(app);
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseServerTiming();
            app.UseResponseCompression();

            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    const int durationInSeconds = 60 * 60 * 24;
                    ctx.Context.Response.Headers[HeaderNames.CacheControl] =
                        "public,max-age=" + durationInSeconds;
                }
            });
            
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }
        
        private static void InitializeDatabase(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.GetService<IServiceScopeFactory>()?.CreateScope();

            var ctx = scope?.ServiceProvider.GetRequiredService<DBContext>();
            ctx?.Database.Migrate();
        }
    }
}
