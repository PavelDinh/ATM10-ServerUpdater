namespace DiscordAPI
{
    public interface IDiscordHandler
    {
        Task SendNotificationAsync(CancellationToken token, string customDomain = "", int customPort = 25565);
    }
}