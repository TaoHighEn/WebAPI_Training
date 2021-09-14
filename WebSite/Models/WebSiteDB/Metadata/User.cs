using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebSite.Models.WebSiteDB
{
    [ModelMetadataType(typeof(UserMetadata))]
    public partial class User
    {
        [NotMapped]
        [Display(Name = "User.ConfirmPassword")]
        [Required(ErrorMessage = "User.ConfirmPassword_Required")]
        [Compare("Password", ErrorMessage = "PasswordNotMatch")]
        public string ConfirmPassword { get; set; }
    }
    public partial class UserMetadata
    {
        [Display(Name = "ID")]
        public long ID { get; set; }

        [Display(Name = "User.Account")]
        [Required(ErrorMessage = "User.Account_Required")]
        //帳號字元限6~20碼，英文和數字(中間可包含一個【_】或【.】)。
        [RegularExpression(@"^(?=[^\._]+[\._]?[^\._]+$)[\w\.]{6,20}$", ErrorMessage = "User.Account_RegexError")]
        public string Account { get; set; }
        [Display(Name = "User.Password")]
        [Required(ErrorMessage = "User.Password_Required")]
        //密碼限4~20個字
        [RegularExpression(@"^.{4,20}$", ErrorMessage = "User.Password_RegexError")]
        public string Password { get; set; }
        [Display(Name = "User.Email")]
        [Required(ErrorMessage = "User.Email_Required")]
        [EmailAddress(ErrorMessage = "InvalidEmail")]
        [MaxLength(50)]
        public string Email { get; set; }
        [Display(Name = "User.Name")]
        [Required(ErrorMessage = "User.Name_Required")]
        [MaxLength(20)]
        public string Name { get; set; }
        [Display(Name = "User.Birthday")]
        [Required(ErrorMessage = "User.Birthday_Required")]
        public string Birthday { get; set; }
    }
}