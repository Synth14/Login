using Login.Models.Settings;
using System.Net;
using System.Net.Mail;

namespace Login.Services.EmailService
{
    public class EmailService : IEmailService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<EmailService> _logger;
        private readonly EmailSettings _emailSettings;

        public EmailService(IWebHostEnvironment environment, ILogger<EmailService> logger, EmailSettings emailSettings)
        {
            _emailSettings = emailSettings;
            _environment = environment;
            _logger = logger;
        }

        public async Task SendAccountConfirmationEmailAsync(string username, Uri callbackUri)
        {
            string subject = "Validation de votre compte";
            string templatePath = Path.Combine(_environment.ContentRootPath, _emailSettings.TemplatePath.TrimStart('/'), "AccountConfirmation.html");

            _logger.LogInformation($"Attempting to read template from: {templatePath}");

            if (!File.Exists(templatePath))
            {
                _logger.LogError($"Template file not found at: {templatePath}");
                throw new FileNotFoundException($"Le fichier de template n'a pas été trouvé à l'emplacement : {templatePath}");
            }

            string template = await File.ReadAllTextAsync(templatePath);
            string body = template
                .Replace("{username}", username)
                .Replace("{validationLink}", callbackUri.ToString());
            await SendEmailAsync(username, subject, body, "Solidr-IT - Validation de compte");
        }

        public async Task SendResetPasswordEmailAsync(string username, Uri callbackUri)
        {
            string subject = "Réinitialisation de mot de passe";
            string templatePath = Path.Combine(_environment.ContentRootPath, _emailSettings.TemplatePath.TrimStart('/'), "ResetPassword.html");

            _logger.LogInformation($"Attempting to read template from: {templatePath}");

            if (!File.Exists(templatePath))
            {
                _logger.LogError($"Template file not found at: {templatePath}");
                throw new FileNotFoundException($"Le fichier de template n'a pas été trouvé à l'emplacement : {templatePath}");
            }

            string template = await File.ReadAllTextAsync(templatePath);
            string body = template
                .Replace("{username}", username)
                .Replace("{resetLink}", callbackUri.ToString());
            await SendEmailAsync(username, subject, body, "Solidr-IT - Réinitialisation du mot de passe");
        }

        private async Task SendEmailAsync(string toEmail, string subject, string htmlBody, string senderDisplayName)
        {
            using (var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort))
            {
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);
                client.EnableSsl = true;

                var message = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, senderDisplayName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true,
                };
                message.To.Add(toEmail);

                await client.SendMailAsync(message);
            }
        }
    }
}