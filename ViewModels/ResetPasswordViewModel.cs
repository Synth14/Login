using System.ComponentModel.DataAnnotations;

namespace Login.ViewModels;

public class ResetPasswordViewModel
{
    [DataType(DataType.Password)]
    [Compare(nameof(PasswordConfirmation))]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    [Compare(nameof(Password))]
    public string PasswordConfirmation { get; set; }

    public List<string> Errors { get; } = new();
}
