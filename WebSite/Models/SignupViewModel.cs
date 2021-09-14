using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using WebSite.Models.WebSiteDB;
using WebSite.Services;

namespace WebSite.Models
{
    public class SignupViewModel : IValidatableObject
    {
        public User User { get; set; }
        public string ErrorMessage { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            using (var serviceScope = ServiceActivator.GetScope())
            {
                ValidatorService validatorService = (ValidatorService)serviceScope.ServiceProvider.GetService(typeof(ValidatorService));

                var rs = validatorService.ValidateSignup(User);
                if (rs != null && rs.Any())
                {
                    var x = rs.Select(r => new ValidationResult(r.Text, new[] { string.IsNullOrEmpty(r.ElementID) ? nameof(ErrorMessage) : r.ElementID }));
                    return x;
                }
            }
            return Enumerable.Empty<ValidationResult>();
        }
    }
}