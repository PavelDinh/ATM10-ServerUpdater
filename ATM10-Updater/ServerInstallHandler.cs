using CurseForgeAPI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ATM10Updater
{
    public class ServerInstallHandler(ILogger<ServerInstallHandler> logger,
                                       IOptions<ModpackInfo> modpackInfo,
                                       IOptions<ServerInfo> serverInfo,
                                       IFileDownloader fileDownloader,
                                       IModpackService modpackService,
                                       IProcessHandler processHandler) : IServerInstallHandler
    {
        public async Task<bool> InstallServer()
        {
            try
            {
                var modFilesJson = await modpackService.GetModFilesAsync(modpackInfo.Value.ModId);

                var (latestVersion, serverId) = VersionHandler.GetLatestVersionAndServerId(modFilesJson);
                var currentVersion = VersionHandler.GetCurrentVersion(serverInfo.Value.LocalServerFolder, serverInfo.Value.NamingConvention);

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
