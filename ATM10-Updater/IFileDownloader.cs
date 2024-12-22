namespace ATM10Updater
{
    public interface IFileDownloader
    {
        Task DownloadFileWithProgressAsync(string fileUrl, string destinationPath, Action<double> reportProgress);
    }
}
