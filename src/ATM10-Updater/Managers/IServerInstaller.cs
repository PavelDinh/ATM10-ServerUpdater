namespace ATM10Updater.Managers
{
    public interface IServerInstaller
    {
        Task<bool> Install();
    }
}
