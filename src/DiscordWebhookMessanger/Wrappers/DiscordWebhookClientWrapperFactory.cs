namespace DiscordAPI.Wrappers
{
    public class DiscordWebhookClientWrapperFactory : IDiscordWebhookClientWrapperFactory
    {
        public IDiscordWebhookClientWrapper Create(string webhookUrl)
        {
            return new DiscordWebhookClientWrapper(webhookUrl);
        }
    }
}
