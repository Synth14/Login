using System.ComponentModel.DataAnnotations;

namespace Login.ViewModels;

public class LoginViewModel
{
    [StringLength(256, ErrorMessage = "UserNameLength")]
    [Required(ErrorMessage = "ViewModel_UserNameRequired")]
    [EmailAddress(ErrorMessage = "EmailAddress")]
    public string UserName { get; set; } = default!;

    [Required(ErrorMessage = "ViewModel_PasswordRequired")]
    public string Password { get; set; } = default!;

    public List<string> Errors { get; } = new();
}
