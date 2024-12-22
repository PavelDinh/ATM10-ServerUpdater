using ATM10Updater.Config;
using ATM10Updater.Handlers;
using ATM10Updater.Providers;
using ATM10Updater.Services;
using CurseForgeAPI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ATM10Updater.Managers
{
    public class ServerInstaller(ILogger<ServerInstaller> logger,
                                       IOptions<ModpackInfo> modpackInfo,
                                       IOptions<ServerInfo> serverInfo,
                                       IFileDownloader fileDownloader,
                                       IModpackService modpackService,
                                       IServerProcessStartup processHandler) : IServerInstaller
    {
        public async Task<bool> Install()
        {
            try
            {
                var modFilesJson = await modpackService.GetModFilesAsync(modpackInfo.Value.ModId);

                var (latestVersion, serverId) = ServerVersionProvider.GetLatestVersionAndServerId(modFilesJson);
                var currentVersion = ServerVersionProvider.GetCurrentVersion(serverInfo.Value.LocalServerFolder, serverInfo.Value.NamingConvention);

                if (currentVersion >= latestVersion)
                {
                    logger.LogInformation("Server file is up to date!");
                    return false;
                }

                var downloadLink = await modpackService.GetDownloadLinkAsync(modpackInfo.Value.ModId, serverId);
                string downloadFilePath = Path.Combine(serverInfo.Value.LocalServerFolder, Path.GetFileName(downloadLink));

                await fileDownloader.DownloadFileWithProgressAsync(downloadLink, downloadFilePath, progress =>
                {
                    logger.LogInformation("\rDownloaded {progress}", $"{progress:P2}");
                });

                Task.Run(() => FileExtractor.ExtractAndOrganize(downloadFilePath, serverInfo.Value.LocalServerFolder, serverInfo.Value.NamingConvention, latestVersion))
                    .Wait();

                await processHandler.StartWarmupProcess();

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during server installation.");
                throw;
            }
        }
    }
}
