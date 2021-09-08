using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WebSite.Models
{
    public class SigninViewModel
    {
        [Display(Name = "User.Account")]
        [Required(ErrorMessage = "帳號必填")]
        public string Account { get; set; }

        [Display(Name = "User.Password")]
        [Required(ErrorMessage = "密碼必填")]
        public string Password { get; set; }

        public string ReturnUrl { get; set; }
        public string ErrorMessage { get; set; }
    }
}
