using ATM10Updater.Config;
using ATM10Updater.Handlers;
using ATM10Updater.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ATM10Updater.Managers
{
    public class ServerInstaller(ILogger<ServerInstaller> logger,
                                       IOptions<ServerConfig> serverInfo,
                                       IFileDownloader fileDownloader,
                                       IServerVersionProvider versionProvider,
                                       IServerMetadataProvider metadataProvider)
        : IServerInstaller
    {
        public async Task<string> InstallAsync(CancellationToken token)
        {
            try
            {
                var metadata = await metadataProvider.GetMetadataAsync(token);
                string downloadFilePath = Path.Combine(serverInfo.Value.LocalServerFolder, Path.GetFileName(metadata.DownloadUrl));

                await fileDownloader.DownloadFileWithProgressAsync(metadata.DownloadUrl, downloadFilePath, progress =>
                {
                    logger.LogInformation("\rDownloaded {progress} [{bytesTransferred} / {totalByts}] - {estimatedCompletionTime}", 
                        $"{progress.ProgressPercentage:P2}", progress.BytesTransferred, progress.TotalBytes, progress.EstimatedTimeRemaining);
                }, 
                token);

                return downloadFilePath;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during server installation.");
                throw;
            }
        }

        public bool IsNewVersionAvailable()
        {
            var latestVersion = versionProvider.GetLatestVersion();
            var currentVersion = versionProvider.GetCurrentVersion();

            // We assume this is fresh install
            if (currentVersion == null)
            {
                return true;
            }

            // Curse Forge API not returning content about latest version
            if (latestVersion == null)
            {
                return false;
            }

            return latestVersion > currentVersion;
        }
    }
}
