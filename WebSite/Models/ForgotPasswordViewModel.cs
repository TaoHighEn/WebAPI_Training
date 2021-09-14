using System.ComponentModel.DataAnnotations;

namespace WebSite.Models
{
    public class ForgotPasswordViewModel
    {
        [Display(Name = "User.Email")]
        [Required(ErrorMessage = "User.Email_Required")]
        [EmailAddress(ErrorMessage = "InvalidEmail")]
        public string Email { get; set; }
        public string ErrorMessage { get; set; }
    }
}