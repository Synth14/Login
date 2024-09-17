using Login.Helpers.Attributes;
using Login.Models.Settings;
using MySqlConnector;
using System.Reflection;
using System.Text.RegularExpressions;

public static class ConfigurationExtensions
{
    public static IServiceCollection ConfigureAppSettings(this IServiceCollection services, IConfiguration configuration)
    {
        var appSettings = new AppSettings();
        configuration.Bind(appSettings);

        ApplyEnvironmentVariables(appSettings);

        services.AddSingleton(appSettings);
        services.AddSingleton(appSettings.Database);
        services.AddSingleton(appSettings.Email);
        services.AddSingleton(appSettings.StockTrackerClient);
        services.AddSingleton(appSettings.Login);

        return services;
    }

    private static void ApplyEnvironmentVariables(object obj)
    {
        foreach (var prop in obj.GetType().GetProperties())
        {
            var attr = prop.GetCustomAttribute<EnvironmentVariableAttribute>();
            if (attr != null)
            {
                var envValue = Environment.GetEnvironmentVariable(attr.Name);
                if (!string.IsNullOrEmpty(envValue))
                {
                    var convertedValue = Convert.ChangeType(envValue, prop.PropertyType);
                    prop.SetValue(obj, convertedValue);
                }
            }

            if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
            {
                ApplyEnvironmentVariables(prop.GetValue(obj));
            }
        }
    }
}