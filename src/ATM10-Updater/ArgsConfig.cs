using ATM10Updater.Handlers;
using ATM10Updater.Managers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace ATM10Updater
{
    public class ArgsConfig : IArgsConfig
    {
        private readonly ILogger<ArgsConfig> _logger;
        private readonly Dictionary<string, Func<CancellationToken, Task>> _argsCmd;

        public ArgsConfig(ILogger<ArgsConfig> logger, IServerProcessHandler processHandler, IServerBackupManager backupManager, IServerInstaller serverInstaller)
        {
            _logger = logger;
            _argsCmd = new Dictionary<string, Func<CancellationToken, Task>>
                {
                    { "--run-server", token => { processHandler.StartProcess(); return Task.CompletedTask; } },
                    { "--run-warmup-process", processHandler.StartWarmupProcessAsync },
                    { "--load-backup", backupManager.LoadBackupAsync },
                    { "--install-server", serverInstaller.InstallAsync },
                };
        }

        public async Task HandleArgsAsync(string[] args, CancellationToken token)
        {
            foreach (var arg in args)
            {
                if (_argsCmd.TryGetValue(arg, out var action))
                {
                    await action(token);
                }
                else
                {
                    _logger.LogWarning("Unknown argument: {Argument}", arg);
                }
            }
        }
    }
}
