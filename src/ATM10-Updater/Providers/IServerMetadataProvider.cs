using ATM10Updater.Data;

namespace ATM10Updater.Providers
{
    public interface IServerMetadataProvider
    {
        ModMetadata GetMetadata();
    }
}