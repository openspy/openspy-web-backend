using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;
using CoreWeb.Database;
using CoreWeb.Models;
using CoreWeb.Models.EA;
using CoreWeb.Repository;
using System.Security.Principal;
using CoreWeb.Authentication;
using CoreWeb.Crypto;
using System.Reflection;
using System.IO;
using StackExchange.Redis;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace CoreWeb
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("CoreService", policy => policy.RequireAssertion(context =>
                {
                    return context.User.HasClaim(c => (c.Type == "role" && c.Value == "Admin") || (c.Type == "role" && c.Value == "CoreService"));
                }));
                options.AddPolicy("Presence", policy => policy.RequireAssertion(context =>
                {
                    return context.User.HasClaim(c => (c.Type == "role" && c.Value == "Admin") || (c.Type == "role" && c.Value == "Presence"));
                }));
                options.AddPolicy("Persist", policy => policy.RequireAssertion(context =>
                {
                    return context.User.HasClaim(c => (c.Type == "role" && c.Value == "Admin") || (c.Type == "role" && c.Value == "Persist") || (c.Type == "role" && c.Value == "ExternalReadOnly"));
                }));
                options.AddPolicy("CDKeyManage", policy => policy.RequireAssertion(context =>
                {
                    return context.User.HasClaim(c => (c.Type == "role" && c.Value == "Admin") || (c.Type == "role" && c.Value == "CoreService"));
                }));

                options.AddPolicy("FESL", policy => policy.RequireAssertion(context =>
                {
                    return context.User.HasClaim(c => (c.Type == "role" && c.Value == "Admin") || (c.Type == "role" && c.Value == "CoreService"));
                }));
                
                options.AddPolicy("GameManage", policy => policy.RequireClaim("role", "Admin"));
                options.AddPolicy("GroupManage", policy => policy.RequireClaim("role", "Admin"));

                options.AddPolicy("ProfileManage", policy => policy.RequireAssertion(context =>
                {
                    return context.User.HasClaim(c => (c.Type == "role" && c.Value == "Admin") || (c.Type == "role" && c.Value == "Presence"));
                }));

                options.AddPolicy("UserManage", policy => policy.RequireAssertion(context =>
                {
                    return context.User.HasClaim(c => (c.Type == "role" && c.Value == "Admin") || (c.Type == "role" && c.Value == "Presence"));
                }));

                options.AddPolicy("UserRegister", policy => policy.RequireAssertion(context =>
                {
                    return context.User.HasClaim(c => (c.Type == "role" && c.Value == "Admin") || (c.Type == "role" && c.Value == "UserRegister"));
                }));

                options.AddPolicy("UserAuth", policy => policy.RequireAssertion(context =>
                {
                    return context.User.HasClaim(c => (c.Type == "role" && c.Value == "Admin") || (c.Type == "role" && c.Value == "UserAuth"));
                }));

                options.AddPolicy("APIKeyManage", policy => policy.RequireAssertion(context =>
                {
                    return context.User.HasClaim(c => (c.Type == "role" && c.Value == "Admin"));
                }));
                options.AddPolicy("GeoAccess", policy => policy.RequireAssertion(context =>
                {
                    return context.User.HasClaim(c => (c.Type == "role" && c.Value == "Admin") || (c.Type == "role" && c.Value == "CoreService"));
                }));
                
                //external roles - PersistRead (granted read only persist & profile access)
                options.AddPolicy("PersistWrite", policy => policy.RequireAssertion(context =>
                {
                    return context.User.HasClaim(c => (c.Type == "role" && c.Value == "Admin") || (c.Type == "role" && c.Value == "Persist"));
                }));
                options.AddPolicy("ProfileRead", policy => policy.RequireAssertion(context =>
                {
                    return context.User.HasClaim(c => (c.Type == "role" && c.Value == "Admin") || (c.Type == "role" && c.Value == "Presence") || (c.Type == "role" && c.Value == "ExternalReadOnly"));
                }));
            });
            services.AddAuthentication("ApiKeyAuth").AddScheme<ApiKeyAuthOpts, ApiKeyAuthHandler>("ApiKeyAuth", "ApiKeyAuth", opts => { });

            services.AddMvc(opt =>
            {
                opt.OutputFormatters.RemoveType<HttpNoContentOutputFormatter>();
                opt.Filters.Add(typeof(JsonExceptionFilter));
            }).SetCompatibilityVersion(CompatibilityVersion.Latest)
            .AddNewtonsoftJson()
            .AddJsonOptions(options =>
            {
                //options.SerializerSettings.MaxDepth = 3;
                //options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });

            var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .AddEnvironmentVariables();
            Configuration = builder.Build();
            services.AddSingleton<IConfiguration>(Configuration);

            services.AddDbContext<GameTrackerDBContext>();
            services.AddDbContext<GamemasterDBContext>();
            services.AddDbContext<KeymasterDBContext>();
            services.AddDbContext<PeerchatDBContext>();


            var multiplexer = ConnectionMultiplexer.Connect(Configuration.GetConnectionString("redisCache"));
            
            services.AddSingleton<IConnectionMultiplexer>(provider => multiplexer);
            services.AddScoped<PresenceStatusDatabase, PresenceStatusDatabase>(c => new PresenceStatusDatabase(multiplexer));
            services.AddScoped<SessionCacheDatabase, SessionCacheDatabase>(c => new SessionCacheDatabase(multiplexer));
            services.AddScoped<GameCacheDatabase, GameCacheDatabase>(c => new GameCacheDatabase(multiplexer));
            services.AddScoped<GroupCacheDatabase, GroupCacheDatabase>(c => new GroupCacheDatabase(multiplexer));
            services.AddScoped<PeerchatCacheDatabase, PeerchatCacheDatabase>(c => new PeerchatCacheDatabase(multiplexer));

            services.AddScoped<IRepository<User, UserLookup>, UserRepository>();
            services.AddScoped<IRepository<Profile, ProfileLookup>, ProfileRepository>();
            services.AddScoped<IRepository<Game, GameLookup>, GameRepository>();
            services.AddScoped<IRepository<Group, GroupLookup>, GroupRepository>();
            services.AddScoped<IRepository<Session, SessionLookup>, AuthSessionRepository>();
            services.AddScoped<IRepository<Buddy, BuddyLookup>, BuddyRepository>();
            services.AddScoped<IRepository<Block, BuddyLookup>, BlockRepository>();
            services.AddScoped<IRepository<PresenceProfileStatus, PresenceProfileLookup>, PresenceProfileStatusRepository>();
            services.AddScoped<IRepository<PersistKeyedData, PersistKeyedDataLookup>, PersistKeyedDataRepository>();
            services.AddScoped<IRepository<PersistData, PersistDataLookup>, PersistDataRepository>();
            services.AddScoped<IRepository<CdKey, CdKeyLookup>, CdKeyRepository>();
            services.AddScoped<IRepository<UsermodeRecord, UsermodeLookup>, UsermodeRepository>();
            services.AddScoped<IRepository<ChanpropsRecord, ChanpropsLookup>, ChanpropsRepository>();
            services.AddScoped<IRepository<GlobalOpersRecord, GlobalOpersLookup>, GlobalOpersRepository>();
            services.AddScoped<IRepository<EntitledGameFeature, EntitledGameFeatureLookup>, GameFeatureRepository>();
            services.AddScoped<IRepository<ObjectInventoryItem, ObjectInventoryLookup>, ObjectInventoryRepository>();
            //ObjectInventoryRepository : IRepository<ObjectInventoryItem, ObjectInventoryLookup>

            services.AddScoped<IMQConnectionFactory, rmqConnectionFactory>(); //this means whenever its required, a connection will be made...
            services.AddScoped<ISnapShotDBContext, SnapShotDBContext>(c => new SnapShotDBContext(Configuration.GetConnectionString("snapshotDB")));
            services.AddScoped<IRepository<Snapshot, SnapshotLookup>, SnapShotRepository>();
            services.AddScoped<IRepository<PlayerProgress, PlayerProgressLookup>, PlayerProgressRepository>();
            services.AddScoped<IRepository<Leaderboard, LeaderboardLookup>, LeaderboardRepository>();

            services.AddSingleton<APIKeyProvider>(c => new APIKeyProvider(Configuration.GetValue<string>("APIKeyPrivateKey")));
            //weak RSA provider... 256 bit key length... due to GP max ticket length = 255 bytes...
            services.AddSingleton<PresencePreAuthProvider>(c => new PresencePreAuthProvider(Configuration.GetValue<string>("PresencePreAuthPrivateKey")));


            services.AddSwaggerGen();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            app.UseAuthentication();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.)
            app.UseSwaggerUI();

            app.UseMvc(routes =>
            {
                routes.MapRoute("v1", "/v1/{controller=Values}/{id?}");
            });
        }
    }
}
