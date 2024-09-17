using Login.Helpers.Attributes;

namespace Login.Models.Settings
{
    public class StockTrackerClientSettings
    {
        [EnvironmentVariable("STOCKTRACKERCLIENT_BASEURL")]
        public string BaseURL { get; set; } = "http://localhost:8433";
    }
}