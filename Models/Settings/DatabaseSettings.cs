namespace Login.Models.Settings
{
    using Login.Helpers.Attributes;
    using System.ComponentModel.DataAnnotations;

    public class DatabaseSettings
    {
        [Required, EnvironmentVariable("DB_SERVER")]
        public string Server { get; set; } = "localhost";

        [Required, EnvironmentVariable("DB_PORT")]
        public int Port { get; set; } = 3306;

        [Required, EnvironmentVariable("DB_NAME")]
        public string Database { get; set; } = "defaultdb";

        [Required, EnvironmentVariable("DB_USER")]
        public string User { get; set; } = "root";

        [Required, EnvironmentVariable("DB_PASSWORD")]
        public string Password { get; set; } = string.Empty;

        public string ConnectionString => $"Server={Server};Port={Port};Database={Database};User={User};Password={Password};";
    }
}