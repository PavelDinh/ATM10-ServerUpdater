using ATM10Updater.Data;
using CurseForgeAPI;
using CurseForgeAPI.Config;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ATM10Updater.Providers
{
    public class ServerMetadataProvider(IOptions<ModpackConfig> modpackInfo, ICurseForgeClient curseForgeClient) 
        : IServerMetadataProvider
    {
        private readonly ModpackConfig modpackInfo = modpackInfo.Value;
        private readonly ICurseForgeClient curseForgeClient = curseForgeClient;
        private ModMetadata? metadataInfo;

        public async ValueTask<ModMetadata> GetMetadataAsync(CancellationToken token)
        {
            if (metadataInfo != null)
            {
                return metadataInfo;
            }

            var modFilesData = await curseForgeClient.GetModFilesAsync(modpackInfo.ModId, token) ?? throw new NullReferenceException("Metadata was not successfully pulled from API");
            JObject modFilesObject = JsonConvert.DeserializeObject<JObject>(modFilesData)!;
            JToken firstElement = modFilesObject["data"]?.First!;
            metadataInfo = firstElement?.ToObject<ModMetadata>();

            var downloadData = await curseForgeClient.GetDownloadFileAsync(modpackInfo.ModId, metadataInfo!.ServerId, token) ?? throw new NullReferenceException("Metadata was not successfully pulled from API");
            var downloadDataObject = JsonConvert.DeserializeObject<JObject>(downloadData)!;
            metadataInfo.DownloadLink = downloadDataObject["data"]?.ToString()!;

            return metadataInfo ?? throw new NullReferenceException("Metadata was not successfully pulled from API");
        }

        public ModMetadata GetMetadata()
        {
            if (metadataInfo == null)
            {
                var task = GetMetadataAsync(CancellationToken.None).AsTask();
                metadataInfo = task.GetAwaiter().GetResult();
            }

            return metadataInfo;
        }
    }
}
