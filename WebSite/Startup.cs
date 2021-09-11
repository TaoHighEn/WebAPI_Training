using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebSite.Models.WebSiteDB;
using WebSite.Resources;
using WebSite.Services;

namespace WebSite
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
            //services �̩ۨʪ`�J (DI)
            //�]�w�s�u�r��
            services.AddDbContext<WebsiteDBContext>(options =>
            {
                options.UseSqlite(Configuration.GetConnectionString("WebsiteDB"));
            });
            //���U
            services.AddControllersWithViews();

            //�h��y�t
            services.AddLocalization();
            services.AddControllersWithViews()
                //�b cshtml ���ϥΦh��y��
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                //�b Model ���ϥΦh��y��
                .AddDataAnnotationsLocalization(
                options =>
                {
                    options.DataAnnotationLocalizerProvider = (type, factory) =>
                    factory.Create(typeof(Resource));
                })
                //Razor���涥�q�sĶ
                .AddRazorRuntimeCompilation();

            // �ϥ� Cookie
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                //�s���Q���������
                options.AccessDeniedPath = new PathString("/Account/Signin");
                //�n�J��
                options.LoginPath = new PathString("/Account/Signin");
                //�n�X��
                options.LogoutPath = new PathString("/Account/Signout");
                //���Įɶ�
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
            });
            
            services.AddHttpContextAccessor();
            //���U�A��
            services.AddScoped<SiteService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            ServiceActivator.Configure(app.ApplicationServices);

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

            //�q��Ʈw�����o�y�t
            using (var serviceScope = ServiceActivator.GetScope())
            {
                SiteService siteService = (SiteService)serviceScope.ServiceProvider.GetService(typeof(SiteService));
                var cultures = siteService.GetCultures();
                var localizationOptions = new RequestLocalizationOptions().SetDefaultCulture(cultures[0])
                    .AddSupportedCultures(cultures)
                    .AddSupportedUICultures(cultures);
                localizationOptions.RequestCultureProviders = new List<IRequestCultureProvider>
                {
                    new QueryStringRequestCultureProvider(),
                    new CookieRequestCultureProvider(),
                    new AcceptLanguageHeaderRequestCultureProvider(),
                };
                app.UseRequestLocalization(localizationOptions);
            }

            app.UseRouting();

            //������
            app.UseAuthentication();
            //�A���v
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

        }
    }
}
