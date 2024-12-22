using System.Text.Json;

namespace ATM10Updater
{
    public static class VersionHandler
    {
        public static (Version version, int serverId) GetLatestVersionAndServerId(JsonDocument modFilesJson)
        {
            var dataArray = modFilesJson.RootElement.GetProperty("data");
            if (dataArray.GetArrayLength() <= 0)
                throw new Exception("No data content received from CurseForge API.");

            var latestFile = dataArray[0];
            var version = ParseVersionFromFileName(Path.GetFileNameWithoutExtension(latestFile.GetProperty("fileName").GetString())!);
            var serverId = latestFile.GetProperty("serverPackFileId").GetInt32();

            return (version, serverId);
        }

        public static Version? GetCurrentVersion(string serverFolder, string namingConvention)
        {
            return Directory.GetDirectories(serverFolder, $"{namingConvention}*")
                .Select(s => ParseVersionFromFileName(Path.GetFileName(s)))
                .OrderByDescending(v => v)
                .FirstOrDefault();
        }

        private static Version ParseVersionFromFileName(string fileName)
        {
            var versionString = fileName.Split('-').Last();
            return Version.Parse(versionString);
        }
    }
}
