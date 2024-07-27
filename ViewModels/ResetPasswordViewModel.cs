using System.ComponentModel.DataAnnotations;

namespace Login.ViewModels;

public class ResetPasswordViewModel
{
    [DataType(DataType.Password)]
    [Compare(nameof(PasswordConfirmation))]
    [Required(ErrorMessage = "ViewModel_PasswordRequired")]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{6,}$",
        ErrorMessage = "PasswordPrerequisite")]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    [Compare(nameof(Password))]
    [Required(ErrorMessage = "ViewModel_PasswordRequired")]
    public string PasswordConfirmation { get; set; }

    public List<string> Errors { get; } = new();
}
