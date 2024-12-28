namespace DiscordAPI.Wrappers
{
    public interface IDiscordWebhookClientWrapperFactory
    {
        IDiscordWebhookClientWrapper Create(string webhookUrl);
    }
}
