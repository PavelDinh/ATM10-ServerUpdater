namespace ATM10Updater
{
    public interface IServerInstallHandler
    {
        Task<bool> InstallServer();
    }
}
