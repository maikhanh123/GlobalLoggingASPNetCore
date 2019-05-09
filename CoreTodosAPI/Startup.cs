using CoreFlogger;
using CoreTodosAPI.Models;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CoreTodosAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ToDoDbContext>(options =>
                options.UseSqlServer(Environment.GetEnvironmentVariable("TODO_CONNSTR")));

            services.AddMvc(options =>
                options.Filters.Add(new TrackPerformanceFilter("ToDos", "Core API")));

            services.AddAuthorization();

            // Securing ASP.NET Core with OAuth2 and OpenID Connect by Kevin Dockx
            // NOTE:  getting identity claims in the access token requires 
            //    config on the identity server -- not done in the demo but you can do in your own
            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = Environment.GetEnvironmentVariable("AUTHORITY");
                    options.RequireHttpsMetadata = false;
                    options.ApiName = "api";
                    options.ApiSecret = "secret";
                });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "ToDos API", Version = "v1" });
                var authority = Environment.GetEnvironmentVariable("AUTHORITY");
                c.AddSecurityDefinition("oauth2", new OAuth2Scheme
                {
                    Type = "oauth2",
                    Flow = "implicit",
                    AuthorizationUrl = $"{authority}/connect/authorize",
                    Scopes = new Dictionary<string, string>
                    {
                        { "api", "Access to the API" },                        
                    }
                });
                
                c.OperationFilter<SwaggerSecurityOperationFilter>("api");                
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {            
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "ToDos API");
                c.ConfigureOAuth2("implicit", "secret", "swagger-ui-realm", "Swagger UI");

            });

            app.UseAuthentication();

            app.UseExceptionHandler(eApp =>
            {
                eApp.Run(async context =>
                {
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "application/json";

                    var errorCtx = context.Features.Get<IExceptionHandlerFeature>();
                    if (errorCtx != null)
                    {
                        var ex = errorCtx.Error;
                        WebHelper.LogWebError("ToDos", "Core API", ex, context);

                        var errorId = Activity.Current?.Id ?? context.TraceIdentifier;
                        var jsonResponse = JsonConvert.SerializeObject(new CustomErrorResponse
                        {
                            ErrorId = errorId,
                            Message = "Some kind of error happened in the API."
                        });
                        await context.Response.WriteAsync(jsonResponse, Encoding.UTF8);
                    }
                });
            });

            app.UseMvc();
        }
    }
}
