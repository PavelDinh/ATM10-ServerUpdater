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
                logger.LogInformation("Found new server version : {modpackName}{version}.", serverInfo.Value.NamingConvention, versionProvider.GetLatestVersion()!.ToString());
                logger.LogInformation("Downloading latest server files.");

                var downloadFilePath = await serverInstaller.InstallAsync();
                await ExtractAndRenameServerFolder(downloadFilePath);

                await processHandler.StartWarmupProcessAsync();
                await backupHandler.LoadBackupAsync();
                await discordHandler.SendNotificationAsync(serverInfo.Value.CustomDomain);
            }

            logger.LogInformation("Starting server.");

            await processHandler.StartProcessAsync();
        }

        private async Task ExtractAndRenameServerFolder(string downloadFilePath)
        {
            await Task.Run(() =>
            {
                var extractFolder = fileExtractor.DecideExtractFolderTarget(downloadFilePath, serverInfo.Value.LocalServerFolder);

                fileExtractor.ExtractZipFile(downloadFilePath, extractFolder);

                var latestVersionFolderName = serverInfo.Value.NamingConvention + versionProvider.GetLatestVersion()?.ToString()!;
                var extractedTargetFolder = Path.Combine(Path.GetDirectoryName(downloadFilePath)!, Path.GetFileNameWithoutExtension(downloadFilePath));
                fileExtractor.RenameFolder(extractedTargetFolder, latestVersionFolderName, true);

                var renameFolderPath = Path.Combine(serverInfo.Value.LocalServerFolder, latestVersionFolderName);
                Environment.SetEnvironmentVariable(serverInfo.Value.ServerFileEnv, renameFolderPath, EnvironmentVariableTarget.User);

                File.Delete(downloadFilePath);
            });
        }
    }
}
