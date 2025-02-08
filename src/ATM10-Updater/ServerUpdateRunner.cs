using ATM10Updater.Config;
using ATM10Updater.Handlers;
using ATM10Updater.Managers;
using ATM10Updater.Providers;
using DiscordAPI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ATM10Updater
{
    public class ServerUpdateRunner(
        ILogger<ServerUpdateRunner> logger,
        IOptions<ServerConfig> serverInfo,
        IServerInstaller serverInstaller,
        IServerProcessHandler processHandler,
        IServerBackupManager backupHandler,
        IDiscordHandler discordHandler,
        IServerVersionProvider versionProvider,
        IFileExtractor fileExtractor)
        : IServerUpdateRunner
    {
        public async Task RunAsync()
        {
            processHandler.EnsureProcessTerminated();

            if (serverInstaller.IsNewVersionAvailable())
            {
                logger.LogInformation("Found new server version : {modpackName}{version}.", serverInfo.Value.NamingConvention, versionProvider.GetLatestVersion()!.ToString());
                logger.LogInformation("Downloading latest server files.");

                var downloadFilePath = await serverInstaller.InstallAsync();
                await fileExtractor.ExtractAndRenameServerFolder(downloadFilePath, serverInfo.Value.LocalServerFolder, serverInfo.Value.NamingConvention);

                await processHandler.StartWarmupProcessAsync();
                await backupHandler.LoadBackupAsync();
                await discordHandler.SendNotificationAsync(serverInfo.Value.CustomDomain);
            }

            logger.LogInformation("Starting server.");

            processHandler.StartProcess();
        }
    }
}
