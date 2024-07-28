using Login.Data;
using System.ComponentModel.DataAnnotations;

namespace Login.Helpers.Validators
{

    public class CustomEmailDomainValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is string email)
            {
                var dbContext = (ApplicationDbContext)validationContext.GetService(typeof(ApplicationDbContext))!;
                var allowedDomains = dbContext.AllowedDomains.Select(d => d.DomainName).ToList();

                string domain = email.Split('@').Last();
                if (allowedDomains.Contains(domain))
                {
                    return ValidationResult.Success!;
                }
            }

            return new ValidationResult(ErrorMessage);
        }
    }
}
