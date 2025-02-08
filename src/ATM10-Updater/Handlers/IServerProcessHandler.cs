namespace ATM10Updater.Handlers
{
    public interface IServerProcessHandler
    {
        Task StartWarmupProcessAsync();

        void StartProcess();

        void EnsureProcessTerminated();
    }
}
