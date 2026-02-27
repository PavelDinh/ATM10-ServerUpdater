namespace ATM10Updater.Handlers
{
    public class FileDownloader(HttpClient client) : IFileDownloader
    {
        public async Task DownloadFileWithProgressAsync(string fileUrl, string destinationPath, Action<DownloadProgress> reportProgress, CancellationToken token, int bufferSize = 8192)
        {
            using var response = await client.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead, token);
            response.EnsureSuccessStatusCode();

            long totalBytes = response.Content.Headers.ContentLength ?? -1;
            using var contentStream = await response.Content.ReadAsStreamAsync(token);
            using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, true);

            long totalRead = 0;
            byte[] buffer = new byte[bufferSize];
            int bytesRead;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            double totalSeconds = 0;
            while ((bytesRead = await contentStream.ReadAsync(buffer, token)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), token);
                totalRead += bytesRead;

                var elapsed = stopwatch.Elapsed;
                var bytesPerSecond = totalRead / elapsed.TotalSeconds;
                var estimatedTimeRemaining = TimeSpan.FromSeconds((totalBytes - totalRead) / bytesPerSecond);

               
                if (elapsed.TotalSeconds - totalSeconds >= 1)
                {
                    totalSeconds = elapsed.TotalSeconds;
                    var progress = new DownloadProgress
                    (
                        ProgressPercentage: (double)totalRead / totalBytes,
                        EstimatedTimeRemaining: estimatedTimeRemaining,
                        BytesTransferred: totalRead,
                        TotalBytes: totalBytes
                    );

                    reportProgress(progress);
                }
            }
        }
    }
}
