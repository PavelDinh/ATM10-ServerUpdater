using Microsoft.Extensions.Options;
using System.Text.Json;

namespace CurseForgeAPI
{
    public class CurseForgeClient : ICurseForgeClient, IDisposable
    {
        private readonly CurseForgeConfig config;

        private readonly HttpClient httpClient;
        
        public CurseForgeClient(IOptions<CurseForgeConfig> options, HttpClient httpClient)
        {
            config = options.Value;
            this.httpClient = httpClient;
            this.httpClient.DefaultRequestHeaders.Add("x-api-key", config.ApiKey);
            this.httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            this.httpClient.BaseAddress = new Uri(config.Endpoint!);
        }

        public void Dispose()
        {
            httpClient.Dispose();
            GC.SuppressFinalize(this);
        }

        public async Task<string> GetDownloadFileAsync(int modId, int fileId)
        {
            var responseContent = await httpClient.GetAsync($"/v1/mods/{modId}/files/{fileId}/download-url").Result.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseContent);
            JsonElement root = doc.RootElement;

            root.TryGetProperty("data", out JsonElement urlElement);

            return urlElement.GetString()!;
        }

        public async Task<string> GetMod(int modId)
        {
            var response = await httpClient.GetAsync($"/v1/mods/{modId}");
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetModFileChangelogAsync(int modId, int fileId)
        {
            var response = await httpClient.GetAsync($"/v1/mods/{modId}/files/{fileId}/changelog");
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetModFilesAsync(int modId)
        {
            var response = await httpClient.GetAsync($"v1/mods/{modId}/files");
            return await response.Content.ReadAsStringAsync();
        }
    }
}
