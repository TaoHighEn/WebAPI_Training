using WebSite.Models.WebSiteDB;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.Linq;
using WebSite.Resources;

namespace WebSite.Services
{
    public class ValidatorMessage
    {
        public string ElementID { get; set; } = string.Empty;
        public string Text { get; set; }
    }
    public class ValidatorService
    {
        private readonly WebsiteDBContext _webSiteDBContext;
        private readonly IStringLocalizer<Resource> _localizer;
        public ValidatorService(WebsiteDBContext webSiteDBContext,
            IStringLocalizer<Resource> localizer)
        {
            _webSiteDBContext = webSiteDBContext;
            _localizer = localizer;
        }
        /// <summary>
        /// SignupViewModel資料驗證
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public IEnumerable<ValidatorMessage> ValidateSignup(User user)
        {
            var result = new List<ValidatorMessage>();

            //檢查帳號是否重覆
            var accountDupes = _webSiteDBContext.User
                .Where(s => (user.ID != null && s.ID.ToLower() != user.ID.ToLower())
                    && s.Account.ToLower() == user.Account.Trim().ToLower()).Select(s => s);
            if (accountDupes.Any())
            {
                result.Add(new ValidatorMessage
                {
                    ElementID = "User.Account",
                    Text = _localizer["User.Account_Used"],
                });
            }

            //檢查Email是否重覆
            var emailDupes = _webSiteDBContext.User
                .Where(s => (user.ID != null && s.ID.ToLower() != user.ID.ToLower())
                    && s.Email.ToLower() == user.Email.Trim().ToLower()).Select(s => s);
            if (emailDupes.Any())
            {
                result.Add(new ValidatorMessage
                {
                    ElementID = "User.Email",
                    Text = _localizer["User.Email_Used"],
                });
            }
            return result;
        }
    }
}