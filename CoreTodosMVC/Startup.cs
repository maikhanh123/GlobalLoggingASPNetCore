using CoreFlogger;
using CoreTodosMVC.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CoreTodosMVC
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

            // Securing ASP.NET Core with OAuth2 and OpenID Connect by Kevin Dockx
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddOpenIdConnect(options =>
            {
                options.Authority = Environment.GetEnvironmentVariable("AUTHORITY");
                options.ClientId = Environment.GetEnvironmentVariable("CLIENT_ID");
                options.ClientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET");

                options.SaveTokens = true;
                options.RequireHttpsMetadata = true;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.ResponseType = "code id_token";

                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("api");
                options.Scope.Add("email");
                options.Scope.Add("offline_access");

                options.ClaimActions.Remove("amr");
            });

            services.AddMvc(options =>
                options.Filters.Add(new TrackPerformanceFilter("ToDos", "Core MVC")));
        }
        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseAuthentication();

            // replace whole conditional with something like 
            // app.UseCustomExceptionHandler("ToDos", "Core MVC", "/Home/Error");
            //app.UseCustomExceptionHandler("ToDos", "Core MVC", "/Home/Error");
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();            

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }        
    }
}
