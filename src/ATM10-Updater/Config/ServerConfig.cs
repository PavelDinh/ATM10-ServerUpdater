namespace ATM10Updater.Config
{
    public class ServerConfig
    {
        public string LocalServerFolder { get; set; } = string.Empty;

        public string NamingConvention { get; set; } = string.Empty;

        public string ServerStartupFile { get; set; } = string.Empty;

        public string ServerRunFile { get; set; } = string.Empty;

        public List<string> BackupFiles { get; set; } = [];

        public string CustomDomain { get; set; } = string.Empty;
    }
}
