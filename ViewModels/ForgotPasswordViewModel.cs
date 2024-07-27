using System.ComponentModel.DataAnnotations;

namespace Login.ViewModels;

public class ForgotPasswordViewModel
{
    [StringLength(256, ErrorMessage = "UserNameLength")]
    [Required(ErrorMessage = "ViewModel_UserNameRequired")]
    [EmailAddress(ErrorMessage = "EmailAddress")]
    public string UserName { get; set; } = default!;
}
