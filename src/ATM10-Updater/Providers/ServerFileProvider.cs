using ATM10Updater.Config;
using Microsoft.Extensions.Options;

namespace ATM10Updater.Providers
{
    public class ServerFileProvider(IOptions<ServerConfig> serverInfo) : IServerFileProvider
    {
        private readonly ServerConfig _serverInfo = serverInfo.Value;

        public IEnumerable<string> GetServerFilesSortedByVersion()
        {
            if (string.IsNullOrWhiteSpace(_serverInfo.LocalServerFolder))
            {
                return [];
            }

            if (!Directory.Exists(_serverInfo.LocalServerFolder))
            {
                return [];
            }

            var serverFolders = Directory.GetDirectories(_serverInfo.LocalServerFolder, $"{_serverInfo.NamingConvention}*");

            return serverFolders
                .Select(path => new { Path = path, Version = TryParseVersion(Path.GetFileName(path)) })
                .Where(entry => entry.Version != null)
                .OrderByDescending(entry => entry.Version)
                .Select(entry => entry.Path)
                .ToList();
        }

        private static Version? TryParseVersion(string? folderName)
        {
            if (string.IsNullOrWhiteSpace(folderName))
            {
                return null;
            }

            var versionString = folderName.Split('-').Last();
            return Version.TryParse(versionString, out var version) ? version : null;
        }
    }
}
