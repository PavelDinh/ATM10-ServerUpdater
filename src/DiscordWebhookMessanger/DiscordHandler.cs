using CurseForgeAPI;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Discord;
using DiscordAPI.Wrappers;
using CurseForgeAPI.Config;

namespace DiscordAPI
{
    public class DiscordHandler(
        IOptions<Config.DiscordConfig> discordInfo,
        IOptions<ModpackConfig> modpackInfo,
        ICurseForgeClient curseForgeClient,
        IDiscordWebhookClientWrapperFactory discordWebhookClientFactory) : IDiscordHandler
    {
        public async Task SendNotificationAsync(string customDomain = "")
        {
            var embed = await ConstructEmbedAsync(customDomain);
            if (embed != null && !string.IsNullOrEmpty(discordInfo.Value.WebhookUrl))
            {
                using var client = discordWebhookClientFactory.Create(discordInfo.Value.WebhookUrl);
                await client.SendMessageAsync(
                    text: "", // Text is optional if the embed contains all necessary content
                    embeds: [embed],
                    username: "ATM-Notifier",
                    avatarUrl: "https://media.forgecdn.net/avatars/thumbnails/1098/957/256/256/638645426291861588.png"
                );
            }
        }

        private async Task<Embed?> ConstructEmbedAsync(string customDomain = "")
        {
            if (string.IsNullOrEmpty(discordInfo.Value.WebhookUrl))
            {
                return null;
            }

            var content = await curseForgeClient.GetModAsync(modpackInfo.Value.ModId);
            var modInfoJson = JsonDocument.Parse(content);
            var dataArray = modInfoJson.RootElement.GetProperty("data").GetProperty("latestFiles");
            var title = dataArray[0].GetProperty("displayName").GetString();
            var id = dataArray[0].GetProperty("id").GetInt32();

            var description = await curseForgeClient.GetModFileChangelogAsync(modpackInfo.Value.ModId, id);
            var changelogData = JsonDocument.Parse(description);
            var changelogContent = changelogData.RootElement.GetProperty("data").GetString();

            var converter = new ReverseMarkdown.Converter();
            var markdownChangelog = converter.Convert(changelogContent);

            var embedBuilder = new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(markdownChangelog)
                .WithUrl($"https://www.curseforge.com/minecraft/modpacks/all-the-mods-10/files/{id}")
                .WithColor(new Color(16705372)) // Yellow
                .WithAuthor("ATM-Notifier", "https://media.forgecdn.net/avatars/thumbnails/1098/957/256/256/638645426291861588.png");

            if (!string.IsNullOrEmpty(customDomain))
            {
                embedBuilder.AddField("Public Server IP :", customDomain);
            }

            return embedBuilder.Build();
        }
    }
}
