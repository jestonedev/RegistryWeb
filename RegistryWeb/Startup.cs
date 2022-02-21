﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RegistryWeb.DataServices;
using System.Globalization;
using System.Collections.Generic;
using RegistryWeb.SecurityServices;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using System;
using RegistryWeb.ReportServices;
using Microsoft.AspNetCore.Http.Features;
using RegistryDb.Interfaces;
using RegistryDb.Models;
using RegistryReformaGKH;

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
                    options.LoginPath = new PathString("/Account/Login");
                    options.AccessDeniedPath = new PathString("/Account/Login");
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

            services.AddTransient<IAuthorizationHandler, CookieUserWithConnectionStringHandler>();

            services.AddAuthorization(opts => {
                var requirements = new List<IAuthorizationRequirement>();
                foreach(var req in opts.DefaultPolicy.Requirements)
                {
                    requirements.Add(req);
                }
                requirements.Add(new CookieUserWithConnectionStringRequirement());
                opts.DefaultPolicy = new AuthorizationPolicy(requirements, opts.DefaultPolicy.AuthenticationSchemes);
            });


            services.AddTransient<BuildingsDataService>();
            services.AddTransient<DocumentIssuerService>();
            services.AddTransient<PremisesDataService>();
            services.AddTransient<OwnerProcessesDataService>();
            services.AddTransient<TenancyProcessesDataService>();
            services.AddTransient<PaymentAccountsDataService>();
            services.AddTransient<ClaimsDataService>();
            services.AddTransient<SecurityService>();
            services.AddTransient<FundsHistoryDataService>();
            services.AddTransient<ReportService>();
            services.AddTransient<OwnerReportService>();
            services.AddTransient<RegistryObjectsReportService>();
            services.AddTransient<RegistryObjectsDataService>();
            services.AddTransient<TenancyObjectsReportService>();
            services.AddTransient<PremiseReportService>();
            services.AddTransient<TenancyReportService>();
            services.AddTransient<ClaimReportService>();
            services.AddTransient<PaymentAccountReportService>();
            services.AddTransient<BuildingReportService>();
            services.AddTransient<ReformaGKHService>();
            services.AddTransient<OwnerReportsDataService>();
            services.AddTransient<PremiseReportsDataService>();
            services.AddTransient<TenancyReportsDataService>();
            services.AddTransient<ClaimReportsDataService>();
            services.AddTransient<PaymentAccountReportsDataService>();
            services.AddTransient<ReestrEmergencyPremisesDataService>();
            services.AddTransient<AddressesDataService>();
            services.AddTransient<PrivatizationDataService>();
            services.AddTransient<PrivatizationReportsDataService>();
            services.AddTransient<PrivatizationReportService>();
            services.AddTransient<PrivRealtorService>();
            services.AddTransient<KumiAccountsDataService>();
            services.AddTransient<KumiPaymentsDataService>();
            services.AddTransient<IDbConnectionSettings, DbConnectionSettings>();
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
