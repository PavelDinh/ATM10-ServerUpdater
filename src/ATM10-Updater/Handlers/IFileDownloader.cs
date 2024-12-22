namespace ATM10Updater.Handlers
{
    public interface IFileDownloader
    {
        Task DownloadFileWithProgressAsync(string fileUrl, string destinationPath, Action<double> reportProgress);
    }
}
