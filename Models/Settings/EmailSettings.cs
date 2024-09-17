using Login.Helpers.Attributes;

namespace Login.Models.Settings
{
    public class EmailSettings
    {
        [EnvironmentVariable("SMTP_SERVER")]
        public string SmtpServer { get; set; } = "smtp.example.com";

        [EnvironmentVariable("SMTP_PORT")]
        public int SmtpPort { get; set; } = 587;

        [EnvironmentVariable("EMAIL_ADDRESS")]
        public string SmtpUsername { get; set; } = "user@example.com";

        [EnvironmentVariable("EMAIL_PASSWORD")]
        public string SmtpPassword { get; set; } = string.Empty;

        [EnvironmentVariable("EMAIL_ADDRESS")]
        public string SenderEmail { get; set; } = "user@example.com";

        public string TemplatePath { get; set; } = "Resources/EmailTemplates";
    }
}