using MySqlConnector;
using System.Text.RegularExpressions;

public static class ConfigurationExtensions
{
    public static string GetEnhancedConnectionString(this IConfiguration configuration, string name)
    {
        var connectionString = configuration.GetConnectionString(name);
        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException($"Connection string '{name}' not found.");

        // Replace environment variables in the connection string
        connectionString = Regex.Replace(connectionString, @"\$\{(\w+)\}", match =>
        {
            var envVarName = match.Groups[1].Value;
            return Environment.GetEnvironmentVariable(envVarName) ?? match.Value;
        });

        var builder = new MySqlConnectionStringBuilder(connectionString);

        // Explicitly set the port if DB_PORT environment variable exists
        if (uint.TryParse(Environment.GetEnvironmentVariable("DB_PORT"), out uint port))
        {
            builder.Port = port;
        }

        return builder.ConnectionString;
    }
}