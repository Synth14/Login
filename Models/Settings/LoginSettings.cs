using Login.Helpers.Attributes;

namespace Login.Models.Settings
{
    public class LoginSettings
    {
        [EnvironmentVariable("LOGIN_BASEURL")]
        public string BaseURL { get; set; } = "http://localhost:7031";
    }
}