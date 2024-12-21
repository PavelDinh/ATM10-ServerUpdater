namespace DiscordWebhookMessanger
{
    public interface IDiscordHandler
    {
        Task SendNotificationAsync(string customDomain);
    }
}