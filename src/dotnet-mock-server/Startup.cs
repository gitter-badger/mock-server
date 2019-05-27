﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace dotnet_mock_server
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddRouting();
            services.AddTransient<IUserRepository, MockUserRepository>();

            //services.AddTransient<MockRepository<User>>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                      ForwardedHeaders.XForwardedProto
            });

            MockConfig mockConfig = MockConfig.ReadConfig("mockServer.json");
            app.UseMockServer(mockConfig);

            return;

            app
                .UseMockServer()

                .On("GET", "/json", "application/json; charset=utf-8", "{}")

                .OnGet("/json", "application/json; charset=utf-8", "{}")

                .OnGet("/json", "application/json; charset=utf-8", (req, resp, route) =>
                {
                    return "{}";
                })

                .On("GET", "/xml", "application/xml", $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<note>
  <to>Tove</to>
  <from>Jani</from>
  <heading>Reminder</heading>
  <body>Don't forget me this weekend!</body>
</note>
")
            .On("GET", "/api/user", "application/json; charset=utf-8", JsonConvert.SerializeObject(app.ApplicationServices.GetService<IUserRepository>().GetAllUsers()))

            .On("GET", "/api/user/1", "application/json; charset=utf-8", JsonConvert.SerializeObject(app.ApplicationServices.GetService<IUserRepository>()[1]))

            .OnGet("/api/user/{id:int}", "application/json; charset=utf-8", (route) =>
            {
                var id = Convert.ToInt32(route.Values["id"]);
                var repo = app.ApplicationServices.GetService<IUserRepository>();
                var user = repo[id];
                return user;
            })

            .OnGet("/api/user/{id:int}", "application/json; charset=utf-8", (route) =>
            {
                var id = Convert.ToInt32(route.Values["id"]);
                var repo = app.ApplicationServices.GetService<IUserRepository>();
                var user = repo[id];
                var content = JsonConvert.SerializeObject(user);
                return content;
            })

            .OnGet("/api/user/{id:int}", "application/json; charset=utf-8", (route) =>
            {
                var id = Convert.ToInt32(route.Values["id"]);
                var repo = app.ApplicationServices.GetService<IUserRepository>();
                var user = repo[id];
                return user;
            })

            .BuildAndUseRouter();
        }
    }
}
