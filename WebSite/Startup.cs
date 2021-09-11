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
            //services 相依性注入 (DI)
            //設定連線字串
            services.AddDbContext<WebsiteDBContext>(options =>
            {
                options.UseSqlite(Configuration.GetConnectionString("WebsiteDB"));
            });
            //註冊
            services.AddControllersWithViews();

            //多國語系
            services.AddLocalization();
            services.AddControllersWithViews()
                //在 cshtml 中使用多國語言
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                //在 Model 中使用多國語言
                .AddDataAnnotationsLocalization(
                options =>
                {
                    options.DataAnnotationLocalizerProvider = (type, factory) =>
                    factory.Create(typeof(Resource));
                })
                //Razor執行階段編譯
                .AddRazorRuntimeCompilation();

            // 使用 Cookie
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                //存取被拒轉跳頁面
                options.AccessDeniedPath = new PathString("/Account/Signin");
                //登入頁
                options.LoginPath = new PathString("/Account/Signin");
                //登出頁
                options.LogoutPath = new PathString("/Account/Signout");
                //有效時間
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
            });
            
            services.AddHttpContextAccessor();
            //註冊服務
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

            //從資料庫中取得語系
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

            //先驗證
            app.UseAuthentication();
            //再授權
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
