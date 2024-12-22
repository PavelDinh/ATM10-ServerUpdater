namespace ATM10Updater.Handlers
{
    public interface IServerProcessStartup
    {
        Task StartWarmupProcess();

        Task StartProcess();
    }
}
