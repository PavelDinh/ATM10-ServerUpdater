using Discord.Webhook;
using Discord;

namespace DiscordAPI.Wrappers
{
    public sealed class DiscordWebhookClientWrapper(string webhookUrl) : IDiscordWebhookClientWrapper
    {
        private readonly DiscordWebhookClient client = new(webhookUrl);

        public Task<ulong> SendMessageAsync(
            string? text = null,
            bool isTTS = false,
            IEnumerable<Embed>? embeds = null,
            string? username = null,
            string? avatarUrl = null,
            RequestOptions? options = null,
            AllowedMentions? allowedMentions = null,
            MessageComponent? components = null,
            MessageFlags flags = MessageFlags.None,
            ulong? threadId = null,
            string? threadName = null,
            ulong[]? appliedTags = null,
            PollProperties? poll = null)
        {
            return client.SendMessageAsync(
                text,
                isTTS,
                embeds,
                username,
                avatarUrl,
                options,
                allowedMentions,
                components,
                flags,
                threadId,
                threadName,
                appliedTags,
                poll);
        }

        public void Dispose()
        {
            client.Dispose();
        }
    }
}
