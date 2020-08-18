using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RegistryWeb.DataServices;
using RegistryWeb.Models;
using System.Globalization;
using System.Collections.Generic;
using RegistryWeb.SecurityServices;
using Microsoft.AspNetCore.Authentication.Cookies;
using System;
using RegistryWeb.ReportServices;
using Microsoft.AspNetCore.Http.Features;

namespace RegistryWeb
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json")
                .Build();
        }

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
            services.Configure<FormOptions>(x => x.ValueCountLimit = int.MaxValue);

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.Cookie.Name = ".RegistryWeb.Session";
                options.IdleTimeout = TimeSpan.FromSeconds(3600);
                options.Cookie.IsEssential = true;
            });

            services.AddTransient<BuildingsDataService>();
            services.AddTransient<PremisesDataService>();
            services.AddTransient<OwnerProcessesDataService>();
            services.AddTransient<TenancyProcessesDataService>();
            services.AddTransient<ChangeLogsDataService>();
            services.AddTransient<SecurityService>();
            services.AddTransient<FundsHistoryDataService>();
            services.AddTransient<ReportService>();
            services.AddTransient<OwnerReportService>();
            services.AddTransient<RegistryObjectsReportService>();
            services.AddTransient<PremiseReportService>();
            services.AddTransient<TenancyReportService>();
            services.AddTransient<BuildingReportService>();
            services.AddTransient<ReformaGKHService>();
            services.AddTransient<OwnerReportsDataService>();
            services.AddTransient<PremiseReportsDataService>();
            services.AddTransient<TenancyReportsDataService>();
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
            app.UseSession();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
