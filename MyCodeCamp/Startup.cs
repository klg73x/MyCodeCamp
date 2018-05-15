using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyCodeCamp.Data;

namespace MyCodeCamp
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            _env = env; //This allows us to save this config to test what environment we are in
            _config = builder.Build();
        }

        private IHostingEnvironment _env;

        IConfigurationRoot _config { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(_config);
            services.AddDbContext<CampContext>(ServiceLifetime.Scoped);
            services.AddScoped<ICampRepository, CampRepository>();
            services.AddTransient<CampDbInitializer>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddAutoMapper();

            //This is a straight usage of Cross Orgin calls on a Global Level. 
            //We are going to do it using policies to allow only the calls and controllers we want 
            //services.AddCors();

            //Cors on a policy non-global level
            services.AddCors(cfg => {
                cfg.AddPolicy("Wildermuth", bldr => {
                    bldr.AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithOrigins("http://wildermuth.com");
                });
                cfg.AddPolicy("AnyGET", bldr => {
                    bldr.AllowAnyHeader()
                    .WithMethods("GET")
                    .AllowAnyOrigin();
                });
            });

            // Add framework services.
            services.AddMvc(opt =>
            {
                //This section is for SSL. Using filters here will be GLOBAL accross all calls to the Api
                //This filter will redirect a standard Http call to the service to instead use the Https version
                if (!_env.IsProduction())
                {
                    opt.SslPort = 44388; //For redirection we need to explicitly tell the dev box which port we want
                                         //In production it will know to use port 443 which is standard
                }
                opt.Filters.Add(new RequireHttpsAttribute());
            })
                .AddJsonOptions(opt =>
                {
                    opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, CampDbInitializer seeder)
        {
            loggerFactory.AddConsole(_config.GetSection("Logging"));
            loggerFactory.AddDebug();

            //This code below is for Cross Origin Calls. So if others in the county need the api we will allow it
            //to come through. This code allows it all globally which is probably what I need.
            //The project wants to use policies so I am remarking it out
            //app.UseCors(cfg =>
            //{
            //    cfg.AllowAnyHeader()
            //        .AllowAnyMethod()
            //        .AllowAnyOrigin();
            //    //.WithOrigins("http://wildermuth.com"); //This is to use an origin specific allow. Intstead of
            //    // allow any
            //});

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc(config =>
            {
                //config.MapRoute("MainAPIRoute", "api/{controller}/{action}");
            });
            seeder.Seed().Wait();
        }
    }
}
