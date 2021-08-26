using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebSite.Models;

namespace WebSite.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Signin(string returnUrl)
        {
            await Task.Yield();
            var model = new SigninViewModel
            {
                //登入後要轉跳的頁面
                ReturnUrl = returnUrl,
            };
            return View(model);
        }
    }
}
