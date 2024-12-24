using ATM10Updater.Config;
using ATM10Updater.Handlers;
using ATM10Updater.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ATM10Updater.Managers
{
    public class ServerInstaller(ILogger<ServerInstaller> logger,
                                       IOptions<ServerInfo> serverInfo,
                                       IFileDownloader fileDownloader,
                                       IServerVersionProvider versionProvider,
                                       IServerMetadataProvider metadataProvider) 
        : IServerInstaller
    {
        public async Task<string> InstallAsync()
        {
            try
            {
                var downloadLink = metadataProvider.GetMetadata().DownloadLink;
                string downloadFilePath = Path.Combine(serverInfo.Value.LocalServerFolder, Path.GetFileName(downloadLink));

                await fileDownloader.DownloadFileWithProgressAsync(downloadLink, downloadFilePath, progress =>
                {
                    logger.LogInformation("\rDownloaded {progress}", $"{progress:P2}");
                });

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

            return latestVersion > currentVersion;
        }
    }
}
