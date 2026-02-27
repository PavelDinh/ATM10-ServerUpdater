namespace ATM10Updater.Handlers
{
    public record DownloadProgress(
        double ProgressPercentage,
        TimeSpan EstimatedTimeRemaining,
        long BytesTransferred,
        long TotalBytes
    );
}
