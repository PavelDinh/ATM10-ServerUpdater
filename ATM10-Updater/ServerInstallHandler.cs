using CurseForgeAPI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO.Compression;
using System.Text.Json;

namespace ATM10Updater
{
    internal class ServerInstallHandler(
        ILogger<ServerInstallHandler> logger,
        IOptions<ModpackInfo> modpackInfo,
        IOptions<ServerInfo> serverInfo,
        HttpClient client,
        ICurseForgeClient curseForgeClient,
        IProcessHandler processHandler) 
        : IServerInstallHandler
    {
        public async Task<bool> InstallServer()
        {
            try
            {
                var modFiles = await curseForgeClient.GetModFilesAsync(modpackInfo.Value.ModId) ?? throw new Exception("Mod files not found.");

                // Get latest modpack files
                var modFilesJson = JsonDocument.Parse(modFiles);
                var dataArray = modFilesJson.RootElement.GetProperty("data");
                if (dataArray.GetArrayLength() <= 0)
                {
                    logger.LogInformation("No data content received from CurseForge API.");
                    return false;
                }
                var serverId = dataArray[0].GetProperty("serverPackFileId").GetInt32();
                var downloadLink = await curseForgeClient.GetDownloadFileAsync(modpackInfo.Value.ModId, serverId) ?? throw new Exception("Server file ID not found.");

                if (!Directory.Exists(serverInfo.Value.LocalServerFolder))
                {
                    Directory.CreateDirectory(serverInfo.Value.LocalServerFolder);
                }

                // Check for latest version
                var currentVersion = Directory
                    .GetDirectories(serverInfo.Value.LocalServerFolder, $"{serverInfo.Value.NamingConvention}*")
                    .Select(s => Version.Parse(s.Split('-').Last()))
                    .OrderByDescending(o => o)
                    .FirstOrDefault();

                Version latestVersion;
                var serverFileZip = downloadLink.Split('/').Last();
                var serverFile = Path.GetFileNameWithoutExtension(serverFileZip);
                latestVersion = Version.Parse(serverFile.Split('-').Last());
                
                if (currentVersion == null || currentVersion >= latestVersion)
                {
                    logger.LogInformation("Server file is up to date!");
                    return false;
                }

                string downloadFilePath = $"{serverInfo.Value.LocalServerFolder}\\{serverFileZip}";

                await DownloadFileWithProgressAsync(downloadLink, downloadFilePath, progress =>
                {
                    Console.Write("\r" + new string(' ', Console.WindowWidth) + "\r");
                    Console.Write($"Downloading server file [{serverFile}] : {progress:P2}");
                });

                logger.LogInformation("Extracting server files.");
                var zipFileLoc = $"{serverInfo.Value.LocalServerFolder}\\{serverFileZip}";
                ZipFile.ExtractToDirectory(zipFileLoc, serverInfo.Value.LocalServerFolder);

                // Once extracted delete zip file
                logger.LogInformation("Deleting zip file.");
                File.Delete(zipFileLoc);

                logger.LogInformation("Renaming server folder.");
                var extractedFolder = Path.Combine(serverInfo.Value.LocalServerFolder, serverFile);
                var folderName = $"{serverInfo.Value.NamingConvention}{latestVersion.ToString(2)}";
                var renamedFolder = Path.Combine(serverInfo.Value.LocalServerFolder, folderName);

                await Task.Run(() => 
                {
                    Environment.SetEnvironmentVariable(serverInfo.Value.EnvironmentName, renamedFolder, EnvironmentVariableTarget.User);
                    Directory.Move(extractedFolder, renamedFolder);
                });

                await processHandler.StartWarmupProcess();

                return true;
            }
            catch
            {
                throw;
            }
        }

        private async Task DownloadFileWithProgressAsync(string fileUrl, string destinationPath, Action<double> reportProgress)
        {
            // Send a HEAD request to get the file size
            using var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, fileUrl));
            response.EnsureSuccessStatusCode();

            long totalBytes = response.Content.Headers.ContentLength ?? -1;
            if (totalBytes == -1)
            {
                throw new Exception("Unable to determine file size.");
            }

            // Download the file
            using var httpResponse = await client.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead);
            httpResponse.EnsureSuccessStatusCode();

            using var contentStream = await httpResponse.Content.ReadAsStreamAsync();
            using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

            long totalRead = 0;
            byte[] buffer = new byte[8192];
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead);
                totalRead += bytesRead;

                // Report progress
                reportProgress((double)totalRead / totalBytes);
            }
        }
    }
}
