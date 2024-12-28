using Discord;

namespace DiscordAPI.Wrappers
{
    public interface IDiscordWebhookClientWrapper : IDisposable
    {
        Task<ulong> SendMessageAsync(
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
            PollProperties? poll = null);
    }
}
