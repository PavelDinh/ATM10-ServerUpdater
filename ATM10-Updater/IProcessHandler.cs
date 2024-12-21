namespace ATM10Updater
{
    internal interface IProcessHandler
    {
        Task StartWarmupProcess();

        Task RunProcess();
    }
}
