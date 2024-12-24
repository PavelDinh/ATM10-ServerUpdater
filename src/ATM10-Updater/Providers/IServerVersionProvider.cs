namespace ATM10Updater.Providers
{
    public interface IServerVersionProvider
    {
        Version? GetLatestVersion();

        Version? GetCurrentVersion();
    }
}