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

        public Task<string> GetDownloadFileAsync(int modId, int fileId, CancellationToken token)
        {
            return SendRequestAsync($"/v1/mods/{modId}/files/{fileId}/download-url", token);
        }

        public Task<string> GetModAsync(int modId, CancellationToken token)
        {
            return SendRequestAsync($"/v1/mods/{modId}", token);
        }

        public Task<string> GetModFileChangelogAsync(int modId, int fileId, CancellationToken token)
        {
            return SendRequestAsync($"/v1/mods/{modId}/files/{fileId}/changelog", token);
        }

        public Task<string> GetModFilesAsync(int modId, CancellationToken token)
        {
            return SendRequestAsync($"/v1/mods/{modId}/files", token);
        }

        private async Task<string> SendRequestAsync(string url, CancellationToken token)
        {
            try
            {
                var response = await httpClient.GetAsync(url, token);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync(token);
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
