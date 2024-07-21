
namespace Login.Services
{
    public interface IEmailService
    {
        Task SendAccountConfirmationEmailAsync(string username, Uri callbackUri);
        Task SendResetPasswordEmailAsync(string username, Uri callbackUri);

    }
}