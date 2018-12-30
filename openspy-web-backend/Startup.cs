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
using CoreWeb.Repository;
using ServiceStack.Redis;

namespace CoreWeb
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
            services.AddMvc(opt =>
            {
                //opt.UseCentralRoutePrefix(new RouteAttribute("v1"));
                opt.Filters.Add(typeof(JsonExceptionFilter));
            }).SetCompatibilityVersion(CompatibilityVersion.Latest)
            .AddJsonOptions(options =>
            {
                options.SerializerSettings.MaxDepth = 3;
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });

            services.AddDbContext<GameTrackerDBContext>();
            services.AddDbContext<GamemasterDBContext>();

            services.AddScoped<IRedisClientsManager>(c =>new BasicRedisClientManager("redis://localhost:6379"));
            services.AddScoped<IRepository<User, UserLookup>, UserRepository>();
            services.AddScoped<IRepository<Profile, ProfileLookup>, ProfileRepository>();
            services.AddScoped<IRepository<Game, GameLookup>, GameRepository>();
            services.AddScoped<IRepository<Group, GroupLookup>, GroupRepository>();
            services.AddScoped<IRepository<Session, SessionLookup>, AuthSessionRepository>();
            services.AddScoped<IMQConnectionFactory, rmqConnectionFactory>(); //this means whenever its required, a connection will be made...


            //weak RSA provider... 256 bit key length... due to GP max ticket length = 255 bytes...
            services.AddSingleton<RSAProvider>(c => new RSAProvider("MIIBOQIBAAJBAIXDkFy6wWnFLkO5egYrB4eZAP2n8goPcyjFcGdw8xLhYHhXuVURjPi2kB+bcNZBGX/FBNfLyKKp+mfW+fQdXCkCASUCQD11h4SNKG7eDlZ30EgF7rPsWma08qqtJxK7lIKN13df8RzPB3nlwMuTMdpvC02xj3gYJ0fBginbGtRxur2hXKECIQD1PQ56QSfP+cjKfx8gyd3sCxn1KMWA5t0D+RPeka9DmwIhAIuiO1v7jEaPJeCm9ApFLARCHCOaYR1gs9on6bIgDuWLAiEA1BkhRyOYDdEcXBrqfTj3SK+nv0XbPwza0wDulvqJve0CIHyJxj1IIyospT38sCTV6PzhBFca/Kt/wwDXfWeEYFAvAiEA7om52jckNWsTLKA1b34ymnXdotJz7HBvXL8/p50RhKg="));


            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "OpenSpy API", Version = "v1" });
                c.CustomSchemaIds(x => x.FullName);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "OpenSpy API V1");
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute("v1", "/v1/{controller=Values}/{id?}");
            });
        }
    }
}
