using System.ComponentModel.DataAnnotations;

namespace Login.ViewModels;

public class AccountNotConfirmedViewModel
{
    public AccountNotConfirmedViewModel()
    {
    }

    public AccountNotConfirmedViewModel(string userName, string returnUrl)
    {
        UserName = userName;
        ReturnUrl = returnUrl;
    }

    [Required]
    [EmailAddress]
    public string? UserName { get; set; }

    public string? ReturnUrl { get; set; }
}
