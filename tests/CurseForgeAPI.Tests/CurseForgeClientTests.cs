using CurseForgeAPI.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;

namespace CurseForgeAPI.Tests
{
    public class CurseForgeClientTests
    {
        const string content = "{\"data\":\"Test\"}";
        private readonly Mock<ILogger<CurseForgeClient>> loggerMock = new();
        private readonly Mock<HttpMessageHandler> mockMessageHandler = new();
        private readonly HttpClient httpClient;
        private readonly Mock<IHttpClientFactory> httpFactoryMock = new();
        private readonly CurseForgeConfig config;
        private readonly IOptions<CurseForgeConfig> optionsConfig;
        private readonly CurseForgeClient curseForgeClient;
        public CurseForgeClientTests()
        {
            config = new CurseForgeConfig
            {
                ApiKey = "test",
                Endpoint = "https://localhost/",
            };
            optionsConfig = Options.Create(config);

            mockMessageHandler.Protected();
            mockMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(content)
                });
            httpClient = new(mockMessageHandler.Object);
            httpFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            curseForgeClient = new CurseForgeClient(loggerMock.Object, optionsConfig, httpFactoryMock.Object);
        }

        [Fact]
        public async Task GetDownloadFileAsync_Returns_DownloadLink()
        {
            var result = await curseForgeClient.GetDownloadFileAsync(123, 123);
            Assert.Equal(content, result);
        }

        [Fact]
        public async Task GetModAsync_Returns_ModData()
        {
            var result = await curseForgeClient.GetModAsync(123);
            Assert.Equal(content, result);
        }

        [Fact]
        public async Task GetModFileChangelogAsync_Returns_Changelog()
        {
            var result = await curseForgeClient.GetModFileChangelogAsync(123, 123);
            Assert.Equal(content, result);
        }

        [Fact]
        public async Task GetModFilesAsync_Returns_DownloadLink()
        {
            var result = await curseForgeClient.GetModFilesAsync(123);
            Assert.Equal(content, result);
        }
    }
}