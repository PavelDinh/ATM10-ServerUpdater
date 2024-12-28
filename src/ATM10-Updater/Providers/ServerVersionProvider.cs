using ATM10Updater.Config;
using Microsoft.Extensions.Options;

namespace ATM10Updater.Providers
{
    public class ServerVersionProvider(IOptions<ServerConfig> serverInfo, IServerMetadataProvider serverMetadataProvider)
        : IServerVersionProvider
    {
        private Version? currentVersion;
        private Version? latestVersion;

        public Version? GetLatestVersion()
        {
            if (latestVersion == null)
            {
                var metadata = serverMetadataProvider.GetMetadata();
                latestVersion = ParseVersionFromFileName(Path.GetFileNameWithoutExtension(metadata.FileName));
            }

            return latestVersion;
        }

        public Version? GetCurrentVersion()
        {
            if (currentVersion == null)
            {
                if (!Directory.Exists(serverInfo.Value.LocalServerFolder))
                {
                    Directory.CreateDirectory(serverInfo.Value.LocalServerFolder);
                }

                currentVersion = Directory.GetDirectories(serverInfo.Value.LocalServerFolder, $"{serverInfo.Value.NamingConvention}*")
                .Select(s => ParseVersionFromFileName(Path.GetFileName(s)))
                .OrderByDescending(v => v)
                .FirstOrDefault();
            }

            return currentVersion;
        }

        private static Version ParseVersionFromFileName(string fileName)
        {
            var versionString = fileName.Split('-').Last();
            return Version.Parse(versionString);
        }
    }
}
