using Moq;
using Microsoft.Extensions.Options;
using Discord;
using CurseForgeAPI;
using DiscordAPI.Wrappers;
using CurseForgeAPI.Config;
using DiscordAPI;

namespace DiscordAPITests
{
    public class DiscordHandlerTests
    {
        private readonly Mock<ICurseForgeClient> curseForgeClientMock;
        private readonly Mock<IOptions<DiscordAPI.Config.DiscordConfig>> discordInfoMock;
        private readonly Mock<IOptions<ModpackConfig>> modpackInfoMock;
        private readonly Mock<IDiscordWebhookClientWrapperFactory> webhookClientWrapperFactoryMock;

        public DiscordHandlerTests()
        {
            curseForgeClientMock = new Mock<ICurseForgeClient>();
            discordInfoMock = new Mock<IOptions<DiscordAPI.Config.DiscordConfig>>();
            modpackInfoMock = new Mock<IOptions<ModpackConfig>>();
            webhookClientWrapperFactoryMock = new Mock<IDiscordWebhookClientWrapperFactory>();
        }

        [Fact]
        public async Task SendToDiscordAsync_ShouldSendMessage_WhenWebhookUrlIsValid()
        {
            // Arrange
            var discordInfo = new DiscordAPI.Config.DiscordConfig { WebhookUrl = "https://valid.webhook.url" };
            discordInfoMock.Setup(x => x.Value).Returns(discordInfo);

            var modpackInfo = new ModpackConfig { ModId = 12345 };
            modpackInfoMock.Setup(x => x.Value).Returns(modpackInfo);

            var modResponse = "{\"data\":{\"latestFiles\":[{\"displayName\":\"Test Mod\",\"id\":67890}]}}";
            curseForgeClientMock.Setup(x => x.GetModAsync(It.IsAny<int>())).ReturnsAsync(modResponse);

            var changelogResponse = "{\"data\":\"Test changelog content\"}";
            curseForgeClientMock.Setup(x => x.GetModFileChangelogAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(changelogResponse);

            var webhookClientWrapperMock = new Mock<IDiscordWebhookClientWrapper>();
            webhookClientWrapperFactoryMock.Setup(x => x.Create(It.IsAny<string>())).Returns(webhookClientWrapperMock.Object);

            var discordHandler = new DiscordHandler(
                discordInfoMock.Object,
                modpackInfoMock.Object,
                curseForgeClientMock.Object,
                webhookClientWrapperFactoryMock.Object);

            // Act
            await discordHandler.SendNotificationAsync("test.example.com");

            // Assert
            webhookClientWrapperMock.Verify(x => x.SendMessageAsync(
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<IEnumerable<Embed>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<RequestOptions>(),
                It.IsAny<AllowedMentions>(),
                It.IsAny<MessageComponent>(),
                It.IsAny<MessageFlags>(),
                It.IsAny<ulong?>(),
                It.IsAny<string>(),
                It.IsAny<ulong[]>(),
                It.IsAny<PollProperties>()), Times.Once);
        }

        [Fact]
        public async Task SendToDiscordAsync_ShouldNotSendMessage_WhenWebhookUrlIsNull()
        {
            // Arrange
            discordInfoMock.Setup(x => x.Value).Returns(new DiscordAPI.Config.DiscordConfig { WebhookUrl = null });

            var discordHandler = new DiscordHandler(
                discordInfoMock.Object,
                modpackInfoMock.Object,
                curseForgeClientMock.Object,
                webhookClientWrapperFactoryMock.Object);

            var webhookClientWrapperMock = new Mock<IDiscordWebhookClientWrapper>();
            webhookClientWrapperFactoryMock.Setup(x => x.Create(It.IsAny<string>())).Returns(webhookClientWrapperMock.Object);

            // Act
            await discordHandler.SendNotificationAsync("test.example.com");

            // Assert
            webhookClientWrapperMock.Verify(x => x.SendMessageAsync(
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<IEnumerable<Embed>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<RequestOptions>(),
                It.IsAny<AllowedMentions>(),
                It.IsAny<MessageComponent>(),
                It.IsAny<MessageFlags>(),
                It.IsAny<ulong?>(),
                It.IsAny<string>(),
                It.IsAny<ulong[]>(),
                It.IsAny<PollProperties>()), Times.Never);
        }
    }
}