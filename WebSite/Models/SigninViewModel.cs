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
        [Required(ErrorMessage = "User.Account_Required")]
        public string Account { get; set; }
        [Display(Name = "User.Password")]
        [Required(ErrorMessage = "User.Password_Required")]
        public string Password { get; set; }
        public string ReturnUrl { get; set; }
        public string ErrorMessage { get; set; }
    }
}
