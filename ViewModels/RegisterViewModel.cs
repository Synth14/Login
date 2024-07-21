using System.ComponentModel.DataAnnotations;

namespace Login.ViewModels;

public class RegisterViewModel
{
    [StringLength(256, ErrorMessage = "UserNameLength")]
    [Required(ErrorMessage = "UserNameRequired")]
    [EmailAddress(ErrorMessage = "EmailAddress")]
    public string UserName { get; set; } = default!;

    [Required(ErrorMessage = "PasswordRequired")]
    public string Password { get; set; } = default!;

    [StringLength(128, ErrorMessage = "FirstNameLength")]
    [Required(ErrorMessage = "FirstNameRequired")]
    public string FirstName { get; set; } = default!;

    [StringLength(128, ErrorMessage = "LastNameLength")]
    [Required(ErrorMessage = "LastNameRequired")]
    public string LastName { get; set; } = default!;

    public List<string> Errors { get; } = new();
}
