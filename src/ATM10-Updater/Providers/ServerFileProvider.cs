using ATM10Updater.Config;
using Microsoft.Extensions.Options;

namespace ATM10Updater.Providers
{
    public class ServerFileProvider(IOptions<ServerConfig> serverInfo) : IServerFileProvider
    {
        private readonly ServerConfig _serverInfo = serverInfo.Value;
        private IEnumerable<string> serverFiles = [];

        public IEnumerable<string> GetServerFilesSortedByVersion()
        {
            if (serverFiles.Any())
            {
                return serverFiles;
            }

            var olderServerFolders = Directory.GetDirectories(_serverInfo.LocalServerFolder, $"{_serverInfo.NamingConvention}*");
            serverFiles = olderServerFolders.OrderByDescending(x => Version.Parse(x.Split('-').Last()));

            return serverFiles;
        }
    }
}
