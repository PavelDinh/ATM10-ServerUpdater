using CurseForgeAPI.Config;
using CurseForgeAPI.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CurseForgeAPI
{
    public class CurseForgeClient : ICurseForgeClient
    {
        private readonly HttpClient httpClient;
        private readonly ILogger<CurseForgeClient> logger;

        public CurseForgeClient(ILogger<CurseForgeClient> logger, IOptions<CurseForgeConfig> options, IHttpClientFactory httpClientFactory)
        {
            this.logger = logger;
            httpClient = httpClientFactory.CreateClient(nameof(CurseForgeClient));
            httpClient.DefaultRequestHeaders.Add("x-api-key", options.Value.ApiKey);
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            httpClient.BaseAddress = new Uri(options.Value.Endpoint!);
        }

        public async Task<string> GetDownloadFileAsync(int modId, int fileId)
        {
            return await SendRequestAsync($"/v1/mods/{modId}/files/{fileId}/download-url");
        }

        public async Task<string> GetModAsync(int modId)
        {
            return await SendRequestAsync($"/v1/mods/{modId}");
        }

        public async Task<string> GetModFileChangelogAsync(int modId, int fileId)
        {
            return await SendRequestAsync($"/v1/mods/{modId}/files/{fileId}/changelog");
        }

        public async Task<string> GetModFilesAsync(int modId)
        {
            return await SendRequestAsync($"/v1/mods/{modId}/files");
        }

        private async Task<string> SendRequestAsync(string url)
        {
            try
            {
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "HTTP error while accessing: {Url}", url);
                throw new CurseForgeApiException($"Failed to fetch data from {url}.", ex);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error while accessing: {Url}", url);
                throw;
            }
        }
    }
}
