using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebSite.Models;
using WebSite.Models.WebSiteDB;
using WebSite.Resources;
using WebSite.Services;

namespace WebSite.Controllers
{
    public class AccountController : Controller
    {
        private readonly SiteService _siteService;
        private readonly WebsiteDBContext _webSiteDBContext;
        private readonly IStringLocalizer<Resource> _localizer;
        private readonly IHttpContextAccessor _context;
        private readonly IViewRenderService _viewRenderService;
        private readonly EmailService _emailService;

        public AccountController(SiteService siteService,
            WebsiteDBContext webSiteDBContext,
            IStringLocalizer<Resource> localizer,
            IHttpContextAccessor context,
            IViewRenderService viewRenderService,
            EmailService emailService)
        {
            _siteService = siteService;
            _webSiteDBContext = webSiteDBContext;
            _localizer = localizer;
            _context = context;
            _viewRenderService = viewRenderService;
            _emailService = emailService;
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

        [HttpPost]
        //防止 CSRF (Cross-Site Request Forgery) 跨站偽造請求的攻擊
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Signin(SigninViewModel model)
        {
            try
            {
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

                //允許 Account 或 Email 登入
                var query = from s in _webSiteDBContext.User
                            where (s.Account.ToLower() == model.Account.Trim().ToLower()
                                 || s.Email.ToLower() == model.Account.Trim().ToLower())
                                && s.Password == _siteService.EncoderSHA512(model.Password)
                            select s;

                if (query == null || !query.Any())
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

        [HttpGet, HttpPost]
        public async Task<IActionResult> SetLanguage(string culture, string returnUrl)
        {
            await Task.Yield();
            _siteService.SetCulture(culture);
            return Redirect(returnUrl);
        }

        [HttpGet]
        public async Task<IActionResult> SignupAsync()
        {
            await Task.Yield();
            var model = new SignupViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignupAsync(SignupViewModel model)
        {

            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.Where(s => s.Errors.Any()).Select(s => s);
                    throw new Exception(errors.First().Errors.First().ErrorMessage);
                }

                model.User.ID = Guid.NewGuid().ToString().ToUpper();
                model.User.Account = model.User.Account.Trim();
                model.User.Password = _siteService.EncoderSHA512(model.User.Password);
                model.User.Name = model.User.Name.Trim();
                model.User.Email = model.User.Email.Trim().ToLower();

                await _webSiteDBContext.User.AddAsync(model.User);
                await _webSiteDBContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                ModelState.AddModelError(nameof(SignupViewModel.ErrorMessage), e.Message);
                return View(model);
            }
            //返回登入頁, 並自動代入所註冊的帳號
            return View("Signin", new SigninViewModel
            {
                Account = model.User.Account,
            });
        }

        [HttpGet]
        public async Task<IActionResult> ForgotPasswordAsync()
        {
            await Task.Yield();
            var model = new ForgotPasswordViewModel();
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPasswordAsync(ForgotPasswordViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.Where(s => s.Errors.Any()).Select(s => s);
                    throw new Exception(errors.First().Errors.First().ErrorMessage);
                }

                //檢查EMAIL是否存在
                /* var user2 = (from a in _webSiteDBContext.User
                              where a.Email.ToLower() == model.Email.Trim().ToLower()
                              select a)?.FirstOrDefault();*/

                var user = _webSiteDBContext.User
                    .Where(a => a.Email.ToLower() == model.Email.Trim().ToLower())
                    .Select(a => a)?.FirstOrDefault();

                if (user == null)
                {
                    throw new Exception(_localizer["NoSuchEmailHere"]);
                }

                //重置密碼信件有效時間
                var expiryMinutes = 30;
                var forgotPassword = new ForgotPassword
                {
                    ID = Guid.NewGuid().ToString(),
                    UserID = user.ID,
                    IsReseted = 0,
                    ExpiryDateTime = DateTime.Now.AddMinutes(expiryMinutes).ToString("yyyy-MM-dd HH:mm:ss.fff"),
                };
                await _webSiteDBContext.ForgotPassword.AddAsync(forgotPassword);
                await _webSiteDBContext.SaveChangesAsync();

                var content = $"{user.Name}，您好";
                content += $"<br/>此連結{expiryMinutes}分鐘內有效";
                content += $"<p/><p/>";

                var tmp = new NotificationMailViewModel
                {
                    Title = $"{_localizer["ResetPassword"]}",
                    Preheader = $"連結 {expiryMinutes}分鐘內有效。",
                    ActionUrl = $"{_context.HttpContext.Request.Scheme}://{_context.HttpContext.Request.Host}/Account/ResetPassword/{forgotPassword.ID}",
                    ActionText = $"{_localizer["ResetPassword"]}",
                    Content = content,
                };
                string template = await _viewRenderService.RenderToStringAsync("~/Views/Account/NotificationMail.cshtml", tmp);

                var mailModel = new MailModel
                {
                    Subject = $"{_localizer["ResetPassword"]}",
                    PlainTextContent = $"{tmp.Preheader}\n{tmp.ActionUrl}",
                    HtmlContent = template,
                    Receivers = new List<EmailAddress> { new EmailAddress(model.Email) },
                };
                var response = await _emailService.Send(mailModel);
                if (!response.IsSuccessStatusCode)
                    throw new Exception(_localizer["FailedToSend"]);

                return View("SentEmail");
            }
            catch (Exception e)
            {
                ModelState.AddModelError(nameof(ForgotPasswordViewModel.ErrorMessage), e.Message);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ResetPasswordAsync(string id)
        {
            await Task.Yield();
            try
            {
                var forgotPassword = _webSiteDBContext.ForgotPassword.Find(id);
                //找不到資料
                if (forgotPassword == null)
                    throw new Exception(_localizer["InvalidLink"]);
                //已經重置過
                if (forgotPassword.IsReseted == 1)
                    throw new Exception(_localizer["ExpiredLink"]);
                //逾時
                if (DateTime.Parse(forgotPassword.ExpiryDateTime) < DateTime.Now)
                    throw new Exception(_localizer["ExpiredLink"]);

                var model = new ResetPasswordViewModel
                {
                    ID = forgotPassword.UserID,
                };
                return View(model);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPasswordAsync(ResetPasswordViewModel model)
        {
            await Task.Yield();
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.Where(s => s.Errors.Any()).Select(s => s);
                    throw new Exception(errors.First().Errors.First().ErrorMessage);
                }
                var forgotPassword = _webSiteDBContext.ForgotPassword.Find(model.ID);
                forgotPassword.IsReseted = 1;
                //重設密碼
                var user = _webSiteDBContext.User.Find(forgotPassword.UserID);
                user.Password = _siteService.EncoderSHA512(model.Password);
                await _webSiteDBContext.SaveChangesAsync();

                return RedirectToAction("Signin", "Account");
            }
            catch (Exception e)
            {
                ModelState.AddModelError(nameof(ResetPasswordViewModel.ErrorMessage), e.Message);
                return View(model);
            }
        }
    }
}
