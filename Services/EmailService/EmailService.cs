namespace Login.Services.EmailService
{
    public class EmailService : IEmailService
    {
        public Task SendAccountConfirmationEmailAsync(string username, Uri callbackUri) => Task.CompletedTask;
        public Task SendResetPasswordEmailAsync(string username, Uri callbackUri) => Task.CompletedTask;
    }
}
