using System.Text.Json;

namespace ATM10Updater.Services
{
    public interface IModpackService
    {
        Task<JsonDocument> GetModFilesAsync(int modId);

        Task<string> GetDownloadLinkAsync(int modId, int serverId);
    }
}