namespace ATM10Updater.Handlers
{
    public class FileDownloader(HttpClient client) : IFileDownloader
    {
        public async Task DownloadFileWithProgressAsync(string fileUrl, string destinationPath, Action<double> reportProgress)
        {
            using var response = await client.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            long totalBytes = response.Content.Headers.ContentLength ?? -1;
            using var contentStream = await response.Content.ReadAsStreamAsync();
            using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

            long totalRead = 0;
            byte[] buffer = new byte[8192];
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                totalRead += bytesRead;
                reportProgress((double)totalRead / totalBytes);
            }
        }
    }
}
