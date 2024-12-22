using System.Text.Json;
using CurseForgeAPI;

namespace ATM10Updater.Services
{
    public class ModpackService(ICurseForgeClient curseForgeClient) : IModpackService
    {
        public async Task<JsonDocument> GetModFilesAsync(int modId)
        {
            var modFiles = await curseForgeClient.GetModFilesAsync(modId);
            return JsonDocument.Parse(modFiles);
        }

        public async Task<string> GetDownloadLinkAsync(int modId, int serverId)
        {
            return await curseForgeClient.GetDownloadFileAsync(modId, serverId);
        }
    }
}
