using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RegistryWeb.Models.Entities;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.DataServices;
using RegistryWeb.Models;
using System.Globalization;
using System.Collections.Generic;
using RegistryWeb.SecurityServices;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace RegistryWeb
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
            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("ru-RU");
                options.SupportedCultures = new List<CultureInfo> { new CultureInfo("ru-RU") };
                options.RequestCultureProviders.Clear();
            });
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options => //CookieAuthenticationOptions
                {
                    options.LoginPath = new Microsoft.AspNetCore.Http.PathString("/Account/Login");
                });

            services.AddDbContext<RegistryContext>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddTransient<BuildingsDataService>();
            services.AddTransient<PremisesListDataService>();
            services.AddTransient<OwnerProcessesDataService>();
            services.AddTransient<ChangeLogsDataService>();
            services.AddTransient<SecurityService>();
            services.AddTransient<ReformaGKHService>();
            services.AddTransient<ReestrEmergencyPremisesDataService>();
            services.AddHttpContextAccessor();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<TokenApiStorage>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
