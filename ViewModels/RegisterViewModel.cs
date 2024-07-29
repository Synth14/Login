using System.ComponentModel.DataAnnotations;
using Login.Helpers.Validators;
namespace Login.ViewModels;

public class RegisterViewModel
{
    [StringLength(256, ErrorMessage = "UserNameLength")]
    [Required(ErrorMessage = "ViewModel_UserNameRequired")]
    [EmailAddress(ErrorMessage = "ViewModelEmailAddress")]
    [CustomEmailDomainValidation(ErrorMessage = "EmailDomainNotAllowed")]
    public string UserName { get; set; } = default!;

    [Required(ErrorMessage = "ViewModel_PasswordRequired")]
    public string Password { get; set; } = default!;

    [StringLength(128, ErrorMessage = "FirstNameLength")]
    [Required(ErrorMessage = "FirstNameRequired")]
    public string FirstName { get; set; } = default!;

    [StringLength(128, ErrorMessage = "LastNameLength")]
    [Required(ErrorMessage = "LastNameRequired")]
    public string LastName { get; set; } = default!;

    public List<string> Errors { get; } = new();
}
