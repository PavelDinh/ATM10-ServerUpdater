using ATM10Updater.Data;
using CurseForgeAPI;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ATM10Updater.Providers
{
    public class ServerMetadataProvider
        : IServerMetadataProvider
    {
        private readonly ModMetadataInfo? metadataInfo;

        public ServerMetadataProvider(IOptions<ModpackInfo> modpackInfo,ICurseForgeClient curseForgeClient)
        {
            var modFilesData = curseForgeClient.GetModFilesAsync(modpackInfo.Value.ModId).GetAwaiter().GetResult();
            JObject modFilesObject = JsonConvert.DeserializeObject<JObject>(modFilesData);
            JToken firstElement = modFilesObject["data"]?.First!;
            metadataInfo = firstElement?.ToObject<ModMetadataInfo>();

            var downloadData = curseForgeClient.GetDownloadFileAsync(modpackInfo.Value.ModId, metadataInfo!.ServerId).GetAwaiter().GetResult();
            var downloadDataObject = JsonConvert.DeserializeObject<JObject>(downloadData);
            metadataInfo.DownloadLink = downloadDataObject["data"]?.ToString()!;
        }

        public ModMetadataInfo GetMetadata()
        {
            return metadataInfo ?? throw new NullReferenceException("Metadata was not successfully pulled from API");
        }
    }
}
