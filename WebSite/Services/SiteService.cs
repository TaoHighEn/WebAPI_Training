using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using System;
using System.Linq;
using WebSite.Models.WebSiteDB;

namespace WebSite.Services
{
    public class SiteService
    {
        private readonly WebsiteDBContext _webSiteDBContext;
        private readonly IHttpContextAccessor _context;

        public SiteService(WebsiteDBContext webSiteDBContext, IHttpContextAccessor context)
        {
            _webSiteDBContext = webSiteDBContext;
            _context = context;
        }

        /// <summary>
        /// 取得語言設定
        /// </summary>
        /// <returns></returns>
        public string[] GetCultures()
        {
            return _webSiteDBContext.Language
                    .Where(s => s.IsEnabled == 1)
                    .OrderBy(s => s.Seq)
                    .Select(s => s.ID)
                    .ToArray();
        }
        /// <summary>
        /// 當前的語言設定
        /// </summary>
        /// <returns></returns>
        public string GetCurrentCulture()
        {
            var cultures = GetCultures();
            var currentCulture = cultures[0];
            if (_context.HttpContext.Request.Cookies.ContainsKey(CookieRequestCultureProvider.DefaultCookieName))
            {
                currentCulture = CookieRequestCultureProvider.ParseCookieValue(_context.HttpContext.Request.Cookies[CookieRequestCultureProvider.DefaultCookieName]).Cultures.FirstOrDefault().Value;
            }
            if (Array.IndexOf(cultures, currentCulture) < 0)
            {
                currentCulture = cultures[0];
            }
            return currentCulture;
        }

        /// <summary>
        /// 設定語言
        /// </summary>
        /// <param name="culture"></param>
        public void SetCulture(string culture = null)
        {
            if (string.IsNullOrEmpty(culture))
            {
                if (_context.HttpContext.Request.Cookies.ContainsKey(CookieRequestCultureProvider.DefaultCookieName))
                {
                    culture = CookieRequestCultureProvider.ParseCookieValue(_context.HttpContext.Request.Cookies[CookieRequestCultureProvider.DefaultCookieName]).Cultures.FirstOrDefault().Value;
                }
                else
                {
                    culture = GetCultures()[0];
                }
            }
            _context.HttpContext.Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );
        }
    }
}