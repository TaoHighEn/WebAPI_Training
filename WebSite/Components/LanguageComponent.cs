using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebSite.Services;

//語言檔
namespace WebSite.Components
{
    [ViewComponent(Name = "Language")]
    public class LanguageComponent : ViewComponent
    {
        private readonly SiteService _siteService;

        public LanguageComponent(SiteService siteService)
        {
            _siteService = siteService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            await Task.Yield();
            return View("Default", _siteService.GetCurrentCulture());
        }

    }
}
