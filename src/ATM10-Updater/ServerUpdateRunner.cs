using ATM10Updater.Config;
using ATM10Updater.Handlers;
using ATM10Updater.Managers;
using ATM10Updater.Providers;
using DiscordWebhookMessanger;
using Microsoft.Extensions.Options;

namespace ATM10Updater
{
    internal class ServerUpdateRunner(
        IOptions<ServerInfo> serverInfo,
        IServerInstaller serverInstaller,
        IServerProcessStartup processHandler,
        IServerBackupManager backupHandler,
        IDiscordHandler discordHandler,
        IServerVersionProvider versionProvider,
        IFileExtractor fileExtractor)
        : IServerUpdateRunner
    {
        public async Task RunAsync()
        {
            if (serverInstaller.IsNewVersionAvailable())
            {
                var downloadFilePath = await serverInstaller.InstallAsync();
                await Task.Run(() =>
                {
                    fileExtractor.ExtractZipFile(downloadFilePath, serverInfo.Value.LocalServerFolder);

                    var extractedFolder = Path.Combine(Path.GetDirectoryName(downloadFilePath)!, Path.GetFileNameWithoutExtension(downloadFilePath));
                    var latestVersionFolderName = serverInfo.Value.NamingConvention + versionProvider.GetLatestVersion()?.ToString()!;
                    fileExtractor.RenameFolder(extractedFolder, latestVersionFolderName, true);
                    
                    var renameFolderPath = Path.Combine(serverInfo.Value.LocalServerFolder, latestVersionFolderName);
                    Environment.SetEnvironmentVariable(serverInfo.Value.ServerFileEnv, renameFolderPath, EnvironmentVariableTarget.User);
                });

                await processHandler.StartWarmupProcessAsync();
                await backupHandler.LoadBackupAsync();
                await discordHandler.SendNotificationAsync(serverInfo.Value.CustomDomain);
            }

            await processHandler.StartProcessAsync();
        }
    }
}
