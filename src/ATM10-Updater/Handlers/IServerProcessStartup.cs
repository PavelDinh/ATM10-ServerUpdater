namespace ATM10Updater.Handlers
{
    public interface IServerProcessStartup
    {
        Task StartWarmupProcessAsync();

        Task StartProcessAsync();
    }
}
