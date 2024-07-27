using System.ComponentModel.DataAnnotations;

namespace Login.ViewModels;

public class ResetPasswordViewModel
{
    [DataType(DataType.Password)]
    [Compare(nameof(PasswordConfirmation))]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{6,}$",
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.")]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    [Compare(nameof(Password))]
    public string PasswordConfirmation { get; set; }

    public List<string> Errors { get; } = new();
}
