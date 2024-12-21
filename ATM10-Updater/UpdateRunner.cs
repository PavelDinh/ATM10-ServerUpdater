using DiscordWebhookMessanger;
using Microsoft.Extensions.Options;

namespace ATM10Updater
{
    internal class UpdateRunner(
        IOptions<ServerInfo> serverInfo,
        IServerInstallHandler serverInstallHandler,
        IProcessHandler processHandler,
        IBackupHandler backupHandler,
        IDiscordHandler discordHandler)
        : IUpdateRunner
    {
        public async Task RunAsync()
        {
            if (await serverInstallHandler.InstallServer())
            {
                await Task.Delay(1000);
                await backupHandler.LoadBackupAsync();
                await discordHandler.SendNotificationAsync(serverInfo.Value.CustomDomain);
            }

            await processHandler.RunProcess();
        }
    }
}
