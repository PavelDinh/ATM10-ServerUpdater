using ATM10Updater.Handlers;
using ATM10Updater.Managers;
using Microsoft.Extensions.Logging;

namespace ATM10Updater
{
    public class ArgsConfig : IArgsConfig
    {
        private readonly ILogger<ArgsConfig> _logger;
        private readonly Dictionary<string, Func<Task>> _argsCmd;

        public ArgsConfig(ILogger<ArgsConfig> logger, IServerProcessHandler processHandler, IServerBackupManager backupManager, IServerInstaller serverInstaller)
        {
            _logger = logger;
            _argsCmd = new Dictionary<string, Func<Task>>
                {
                    { "--start-server", new Func<Task>(() => { processHandler.StartProcess(); return Task.CompletedTask; }) },
                    { "--load-backup", new Func<Task>(backupManager.LoadBackupAsync) },
                    { "--install-server", new Func<Task<string>>(serverInstaller.InstallAsync) }
                };
        }

        public async Task HandleArgsAsync(string[] args)
        {
            foreach (var arg in args)
            {
                if (_argsCmd.TryGetValue(arg, out var action))
                {
                    await action();
                }
                else
                {
                    _logger.LogWarning("Unknown argument: {Argument}", arg);
                }
            }
        }
    }
}
