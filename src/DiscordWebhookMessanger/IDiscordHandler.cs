namespace DiscordAPI
{
    public interface IDiscordHandler
    {
        Task SendNotificationAsync(string customDomain = "", int customPort = 25565);
    }
}