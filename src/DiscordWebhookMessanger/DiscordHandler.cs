using CurseForgeAPI;
using JNogueira.Discord.Webhook.Client;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace DiscordWebhookMessanger
{
    public class DiscordHandler(
        IOptions<DiscordInfo> discordInfo,
        IOptions<ModpackInfo> modpackInfo,
        ICurseForgeClient curseForgeClient) : IDiscordHandler
    {
        public async Task SendNotificationAsync(string customDomain = "")
        {
            var content = await curseForgeClient.GetMod(modpackInfo.Value.ModId);
            var modInfoJson = JsonDocument.Parse(content);
            var dataArray = modInfoJson.RootElement.GetProperty("data").GetProperty("latestFiles");
            var title = dataArray[0].GetProperty("displayName").GetString();
            var id = dataArray[0].GetProperty("id").GetInt32();

            var description = await curseForgeClient.GetModFileChangelogAsync(modpackInfo.Value.ModId, id);
            var changelogData = JsonDocument.Parse(description);
            var changelogContent = changelogData.RootElement.GetProperty("data").GetString();

            var converter = new ReverseMarkdown.Converter();
            var markdownChangelog = converter.Convert(changelogContent);

            var client = new DiscordWebhookClient(discordInfo.Value.WebhookUrl);

            var embedField = !string.IsNullOrEmpty(customDomain) ? new DiscordMessageEmbedField("Public Server IP : ", $"{customDomain}:25566") : new("");

            // Create your DiscordMessage with all parameters of your message.
            var message = new DiscordMessage(
                title,
                username: "ATM-Notifier",
                avatarUrl: "https://media.forgecdn.net/avatars/thumbnails/1098/957/256/256/638645426291861588.png",
                embeds:
                [
                    new DiscordMessageEmbed(
                        "Patch notes",
                        color: 16705372, // Yellow - https://gist.github.com/thomasbnt/b6f455e2c7d743b796917fa3c205f812
                        url: $"https://www.curseforge.com/minecraft/modpacks/all-the-mods-10/files/{id}",
                        description: markdownChangelog,
                        fields:
                        [
                            embedField
                        ])
                ]
            );

            // Send the message!
            await client.SendToDiscord(message);
        }
    }
}
