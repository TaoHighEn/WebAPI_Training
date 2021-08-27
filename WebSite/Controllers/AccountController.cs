using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebSite.Models;

namespace WebSite.Controllers
{
    public class AccountController : Controller
    {
        //public IActionResult Index()
        //{
        //    return View();
        //}

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

        [HttpPost]
        //防止 CSRF (Cross-Site Request Forgery) 跨站偽造請求的攻擊
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Signin(SigninViewModel model)
        {
            try
            {
                //建立測試帳號
                var testUsers = new List<SigninViewModel>
                {
                    new SigninViewModel{
                        Account = "admin",
                        Password = "admin",
                    },
                    new SigninViewModel{
                        Account = "user",
                        Password = "user",
                    },
                };
                //檢查帳號密碼是否正確
                //通常帳號會忽略大小寫
                if (string.IsNullOrEmpty(model.Account))
                {
                    throw new Exception("請輸入帳號");
                }
                if (string.IsNullOrEmpty(model.Password))
                {
                    throw new Exception("請輸入密碼");
                }

                //Lambda
                var sExists2 = (from s in testUsers
                                where s.Account.ToLower() == model.Account.Trim().ToLower() &&
                                s.Password == model.Password
                                select s).Any();
                //LINQ
                var isExists = testUsers.Where(s => s.Account.ToLower() == model.Account.Trim().ToLower()
                    && s.Password == model.Password).Any();
                if (!isExists)
                    throw new Exception("帳號或密碼錯誤");

                // 設定 Cookie
                var claims = new List<Claim>
                {
                     new Claim(ClaimTypes.NameIdentifier, model.Account.Trim().ToLower()),
                };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(principal);

                //沒有指定返回的頁面就導向 /Home/Index
                if (string.IsNullOrEmpty(model.ReturnUrl))
                    return RedirectToAction("Index", "Home");
                else
                    return Redirect(model.ReturnUrl);
            }
            catch (Exception e)
            {
                //錯誤訊息
                ModelState.AddModelError(nameof(SigninViewModel.ErrorMessage), e.Message);
                return View(model);
            }
        }

        [HttpGet, HttpPost]
        public async Task<IActionResult> SignoutAsync([FromQuery] string ReturnUrl)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            foreach (var cookie in HttpContext.Request.Cookies)
            {
                Response.Cookies.Delete(cookie.Key);
            }

            return RedirectToAction("Signin", "Account", new
            {
                returnUrl = ReturnUrl
            });
        }


        [HttpGet]
        public ActionResult Signin2(string returnUrl, int id)
        {
            //await Task.Yield();
            var model = new SigninViewModel
            {
                //登入後要轉跳的頁面
                ReturnUrl = returnUrl,
            };
            if (id == 1)
            {
                System.Threading.Thread.Sleep(10000);
                return View(model);
            }
            else
            {
                return View(model);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Signin3(string returnUrl, int id)
        {
            await Task.Yield();
            if (id == 1)
            {
                await Task.Delay(10000);
            }
            var model = new SigninViewModel
            {
                //登入後要轉跳的頁面
                ReturnUrl = returnUrl,
            };
            return View(model);
        }
    }
}
