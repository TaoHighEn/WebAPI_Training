using System.ComponentModel.DataAnnotations;

namespace WebSite.Models
{
    public class ResetPasswordViewModel
    {
        public string ID { get; set; }
        [Display(Name = "User.Password")]
        [Required(ErrorMessage = "User.Password_Required")]
        //密碼限4~20個字
        [RegularExpression(@"^.{4,20}$", ErrorMessage = "User.Password_RegexError")]
        public string Password { get; set; }
        [Display(Name = "User.ConfirmPassword")]
        [Required(ErrorMessage = "User.ConfirmPassword_Required")]
        [Compare("Password", ErrorMessage = "PasswordNotMatch")]
        public string ConfirmPassword { get; set; }
        public string ErrorMessage { get; set; }
    }
}