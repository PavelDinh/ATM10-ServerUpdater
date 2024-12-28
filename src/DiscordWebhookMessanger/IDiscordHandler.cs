namespace DiscordAPI
{
    public interface IDiscordHandler
    {
        Task SendNotificationAsync(string customDomain = "");
    }
}