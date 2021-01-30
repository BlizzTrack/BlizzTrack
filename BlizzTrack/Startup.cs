using BlizzTrack.Constraint;
using Core.Models;
using Core.Tooling;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement;
using Microsoft.Net.Http.Headers;
using Minio.AspNetCore;
using Newtonsoft.Json.Converters;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Newtonsoft;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlizzTrack
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            var results = GetAllEntities();

            foreach (var result in results)
            {
                var ins = (IGameToolStartup)Activator.CreateInstance(result);
                gameToolStartups.Add(ins);
            }
        }

        public IConfiguration Configuration { get; }

        private List<IGameToolStartup> gameToolStartups = new List<IGameToolStartup>();

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddResponseCompression();

            services.AddSwaggerGen(options =>
            {
                options.DocInclusionPredicate((_, api) => !string.IsNullOrWhiteSpace(api.GroupName));
                options.TagActionsBy(api => new[] { api.GroupName });

                var files = Directory.GetFiles(AppContext.BaseDirectory, "*.xml");
                foreach (var file in files)
                {
                    // var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    // var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                    options.IncludeXmlComments(file);
                }
            }).AddSwaggerGenNewtonsoftSupport();

            services.AddFeatureManagement(Configuration.GetSection("Features"));

            services.Configure<RazorViewEngineOptions>(o =>
            {
                o.ViewLocationFormats.Add
                    ("/Pages/Partials/{0}" + RazorViewEngine.ViewExtension);
                o.PageViewLocationFormats.Add("/Pages/Partials/{0}" + RazorViewEngine.ViewExtension);
            });

            services.Configure<RouteOptions>(options =>
            {
                options.ConstraintMap.Add("enum", typeof(EnumConstraint));
            });

            services.AddControllers().AddNewtonsoftJson(options => options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented).AddXmlSerializerFormatters();
            services.AddControllersWithViews().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
                options.SerializerSettings.Converters.Add(new StringEnumConverter());
            });
            services.AddRazorPages().AddNewtonsoftJson(options => options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented);
            services.Configure<MvcNewtonsoftJsonOptions>(x => x.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore);

            services.AddDefaultIdentity<User>(options => { options.SignIn.RequireConfirmedAccount = false; options.User.RequireUniqueEmail = false; })
                  .AddRoles<IdentityRole>()
                  .AddEntityFrameworkStores<DBContext>();

            services.AddDbContext<DBContext>(options =>
                options.UseNpgsql(
                    Configuration.GetConnectionString("ConnectionString"), o =>
                    {
                        o.UseTrigrams();
                    }).EnableSensitiveDataLogging());

            services.AddMvc(options => options.EnableEndpointRouting = false)
                .SetCompatibilityVersion(CompatibilityVersion.Latest).AddNewtonsoftJson();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            }).AddCookie(options =>
            {
                options.Cookie.Name = "BT.Auth";
                options.Cookie.IsEssential = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
                options.SlidingExpiration = true;

                options.LoginPath = "/Auth/Login";
                options.LogoutPath = "/Auth/Logout";
                options.AccessDeniedPath = "/"; // We don't really care
            })
           .AddBattleNet(options =>
           {
               options.ClientId = Configuration.GetValue("BattleNet:ClientID", "");
               options.ClientSecret = Configuration.GetValue("BattleNet:ClientSecret", "");

               options.SaveTokens = true;
               options.CorrelationCookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Unspecified;

               options.Events = new OAuthEvents
               {
                   OnCreatingTicket = ctx =>
                   {
                       ctx.Identity.AddClaim(new Claim("urn:bnet:access_token", ctx.AccessToken));
                       return Task.CompletedTask;
                   },

                   OnTicketReceived = ctx =>
                   {
                       var username = ctx.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
                       if (string.IsNullOrWhiteSpace(username))
                       {
                           ctx.HandleResponse();
                           ctx.Response.Redirect("/");
                           return Task.CompletedTask;
                       }

                       return Task.CompletedTask;
                   }
               };
           });

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
                Database = 1,
                ServerEnumerationStrategy = new ServerEnumerationStrategy()
                {
                    Mode = ServerEnumerationStrategy.ModeOptions.All,
                    TargetRole = ServerEnumerationStrategy.TargetRoleOptions.Any,
                    UnreachableServerAction = ServerEnumerationStrategy.UnreachableServerActionOptions.IgnoreIfOtherAvailable
                }
            };

            services.AddStackExchangeRedisExtensions<NewtonsoftSerializer>(conf);

            services.AddMinio(options =>
            {
                options.Endpoint = Configuration.GetValue("AWS:Endpoint", "");
                options.SecretKey = Configuration.GetValue("AWS:SecretKey", "");
                options.AccessKey = Configuration.GetValue("AWS:AccessKey", "");
                options.OnClientConfiguration = client =>
                {
                    client.WithSSL();
                };
            });

            // Blizzard services
            services.AddSingleton(x => new BNetLib.Networking.BNetClient());
            services.AddSingleton(x => new BNetLib.Http.ProductConfig());

            // System Services
            services.AddSingleton<Services.IBlizzardAlerts, Services.BlizzardAlerts>();
            services.AddScoped<Services.IPatchnotes, Services.Patchnotes>();

            // Shared services
            services.AddScoped<Core.Services.ISummary, Core.Services.Summary>();
            services.AddScoped<Core.Services.IVersions, Core.Services.Versions>();
            services.AddScoped<Core.Services.ICDNs, Core.Services.CDNs>();
            services.AddScoped<Core.Services.IBGDL, Core.Services.BGDL>();
            services.AddScoped<Core.Services.IGameConfig, Core.Services.GameConfig>();
            services.AddScoped<Core.Services.IGameParents, Core.Services.GameParents>();

            // Load external services
            gameToolStartups.ForEach(x => x.ConfigureServices(ref services));
        }

        public List<Type> GetAllEntities()
        {
            var a = new List<Assembly>();
            var files = Directory.GetFiles(AppContext.BaseDirectory, "*.Tools.dll");
            foreach (var file in files)
            {
                a.Add(Assembly.LoadFrom(file));
            }

            return a.SelectMany(x => x.GetTypes())
                 .Where(x => typeof(IGameToolStartup).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract).ToList();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            InitializeDatabase(app);
            InitializeRoles(app);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

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

            app.UseSwagger();

            app.UseReDoc(c =>
            {
                c.SpecUrl("/swagger/v1/swagger.json");
                c.RoutePrefix = "api";
                c.DocumentTitle = "BlizzTrack API";
                c.RequiredPropsFirst();
                c.NativeScrollbars();
                c.ExpandResponses("all");
            });

            app.UseStatusCodePagesWithReExecute("/errors/{0}");
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMvcWithDefaultRoute();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();

                gameToolStartups.ForEach(x => x.MapEndpoints(endpoints));
            });
        }

        private static void InitializeDatabase(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope();

            var ctx = scope.ServiceProvider.GetRequiredService<DBContext>();
            ctx.Database.Migrate();
        }

        private static void InitializeRoles(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope();

            var ctx = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // first we create Admin rool
            var role = new IdentityRole
            {
                Name = "Admin"
            };
            ctx.CreateAsync(role).Wait();

            var user = new IdentityRole
            {
                Name = "User"
            };
            ctx.CreateAsync(user).Wait();
        }
    }
}