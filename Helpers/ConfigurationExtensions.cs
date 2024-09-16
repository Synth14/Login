using MySqlConnector;

public static class ConfigurationExtensions
{
    public static string GetEnhancedConnectionString(this IConfiguration configuration, string name)
    {
        var connectionString = configuration.GetConnectionString(name);
        var builder = new MySqlConnectionStringBuilder(connectionString);

        if (uint.TryParse(Environment.GetEnvironmentVariable("DB_PORT"), out uint port))
        {
            builder.Port = port;
        }

        return builder.ConnectionString;
    }
}