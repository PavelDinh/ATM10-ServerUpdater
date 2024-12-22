using ATM10Updater.Config;
using ATM10Updater.Handlers;
using ATM10Updater.Managers;
using DiscordWebhookMessanger;
using Microsoft.Extensions.Options;

namespace ATM10Updater
{
    internal class ServerUpdateRunner(
        IOptions<ServerInfo> serverInfo,
        IServerInstaller serverInstaller,
        IServerProcessStartup processHandler,
        IServerBackupManager backupHandler,
        IDiscordHandler discordHandler)
        : IServerUpdateRunner
    {
        public async Task RunAsync()
        {
            if (await serverInstaller.Install())
            {
                await Task.Delay(1000);
                await backupHandler.LoadBackupAsync();
                await discordHandler.SendNotificationAsync(serverInfo.Value.CustomDomain);
            }

            await processHandler.StartProcess();
        }
    }
}
