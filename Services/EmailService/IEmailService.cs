namespace Login.Services.EmailService
{
    public interface IEmailService
    {
        Task SendAccountConfirmationEmailAsync(string username, Uri callbackUri);
        Task SendResetPasswordEmailAsync(string username, Uri callbackUri);

    }
}