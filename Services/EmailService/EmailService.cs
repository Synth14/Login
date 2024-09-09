using System.Net;
using System.Net.Mail;

namespace Login.Services.EmailService
{
    public class EmailService : IEmailService
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _senderEmail;
        private readonly string _templatePath;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, IWebHostEnvironment environment, ILogger<EmailService> logger)
        {
            _smtpServer = configuration["Email:SmtpServer"];
            _smtpPort = int.Parse(configuration["Email:SmtpPort"]);
            _smtpUsername = configuration["Email:SmtpUsername"];
            _smtpPassword = configuration["Email:SmtpPassword"];
            _senderEmail = configuration["Email:SenderEmail"];
            _templatePath = configuration["Email:TemplatePath"];
            _environment = environment;
            _logger = logger;
        }

        public async Task SendAccountConfirmationEmailAsync(string username, Uri callbackUri)
        {
            string subject = "Validation de votre compte";
            string templatePath = Path.Combine(_environment.ContentRootPath, _templatePath.TrimStart('/'), "AccountConfirmation.html");

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
            string templatePath = Path.Combine(_environment.ContentRootPath, _templatePath.TrimStart('/'), "ResetPassword.html");

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
            using (var client = new SmtpClient(_smtpServer, _smtpPort))
            {
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                client.EnableSsl = true;

                var message = new MailMessage
                {
                    From = new MailAddress(_senderEmail, senderDisplayName),
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