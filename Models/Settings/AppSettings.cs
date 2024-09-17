namespace Login.Models.Settings
{
    public class AppSettings
    {
        public DatabaseSettings Database { get; set; } = new DatabaseSettings();
        public EmailSettings Email { get; set; } = new EmailSettings();
        public StockTrackerClientSettings StockTrackerClient { get; set; } = new StockTrackerClientSettings();
        public LoginSettings Login { get; set; } = new LoginSettings();
    }

}
