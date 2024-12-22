namespace ATM10Updater
{
    public interface IProcessHandler
    {
        Task StartWarmupProcess();

        Task RunProcess();
    }
}
