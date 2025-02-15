namespace ATM10Updater.Handlers
{
    public interface IServerProcessHandler
    {
        Task StartWarmupProcessAsync(CancellationToken token);

        void StartProcess();

        void EnsureProcessTerminated();
    }
}
