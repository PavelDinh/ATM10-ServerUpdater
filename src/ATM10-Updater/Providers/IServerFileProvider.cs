namespace ATM10Updater.Providers
{
    public interface IServerFileProvider
    {
        IEnumerable<string> GetServerFilesSortedByVersion();
    }
}