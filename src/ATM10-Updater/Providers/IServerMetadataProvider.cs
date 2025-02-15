using ATM10Updater.Data;

namespace ATM10Updater.Providers
{
    public interface IServerMetadataProvider
    {
        ValueTask<ModMetadata> GetMetadataAsync(CancellationToken token);
        ModMetadata GetMetadata();
    }
}