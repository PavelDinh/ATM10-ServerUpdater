using ATM10Updater.Data;
using CurseForgeAPI;
using CurseForgeAPI.Config;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ATM10Updater.Providers
{
    public class ServerMetadataProvider
        : IServerMetadataProvider
    {
        private readonly ModMetadata? metadataInfo;

        public ServerMetadataProvider(IOptions<ModpackConfig> modpackInfo,ICurseForgeClient curseForgeClient)
        {
            var modFilesData = curseForgeClient.GetModFilesAsync(modpackInfo.Value.ModId).GetAwaiter().GetResult() ?? throw new NullReferenceException("Metadata was not successfully pulled from API");
            JObject modFilesObject = JsonConvert.DeserializeObject<JObject>(modFilesData);
            JToken firstElement = modFilesObject["data"]?.First!;
            metadataInfo = firstElement?.ToObject<ModMetadata>();

            var downloadData = curseForgeClient.GetDownloadFileAsync(modpackInfo.Value.ModId, metadataInfo!.ServerId).GetAwaiter().GetResult() ?? throw new NullReferenceException("Metadata was not successfully pulled from API");
            var downloadDataObject = JsonConvert.DeserializeObject<JObject>(downloadData);
            metadataInfo.DownloadLink = downloadDataObject["data"]?.ToString()!;
        }

        public ModMetadata GetMetadata()
        {
            return metadataInfo ?? throw new NullReferenceException("Metadata was not successfully pulled from API");
        }
    }
}
