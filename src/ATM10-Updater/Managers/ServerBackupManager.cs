using ATM10Updater.Config;
using ATM10Updater.Handlers;
using ATM10Updater.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ATM10Updater.Managers
{
    public class ServerBackupManager(ILogger<ServerBackupManager> logger,
        IOptions<ServerConfig> serverInfo,
        IServerProcessHandler serverProcess,
        IServerFileProvider serverFileProvider) : IServerBackupManager
    {
        private readonly ServerConfig _serverConfig = serverInfo.Value;

        public async Task LoadBackupAsync(CancellationToken token)
        {
            try
            {
                serverProcess.EnsureProcessTerminated();

                var serverFiles = serverFileProvider.GetServerFilesSortedByVersion();

                if (serverFiles.Count() == 1)
                {
                    // Accept eula
                    var eulaTxtPath = $"{serverFiles.First()}\\eula.txt";
                    string eulaContent = await File.ReadAllTextAsync(eulaTxtPath, token);
                    var acceptedEula = eulaContent.Replace("eula=false", "eula=true");
                    await File.WriteAllTextAsync(eulaTxtPath, acceptedEula, token);

                    return;
                }

                var latestServerFolder = serverFiles.ElementAt(0);
                var olderServerFolder = serverFiles.ElementAt(1);

                foreach (var content in _serverConfig.BackupFiles)
                {
                    var oldFile = Path.Combine(olderServerFolder, content);
                    var newDestFile = Path.Combine(latestServerFolder, content);

                    if (File.GetAttributes(oldFile).HasFlag(FileAttributes.Directory))
                    {
                        if (string.IsNullOrWhiteSpace(oldFile))
                        {
                            logger.LogWarning("Folder was not present in Path : [{oldFIle}]", oldFile);
                            continue;
                        }

                        await CopyFolderAsync(oldFile, newDestFile, token);
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(oldFile))
                        {
                            logger.LogWarning("File was not present in Path : [{oldFIle}]", oldFile);
                            continue;
                        }

                        await CopyFileAsync(oldFile, newDestFile, token);
                    }

                }
            }
            catch (OperationCanceledException) 
            {
                throw;
            }
            catch (IOException)
            {
                throw;
            }
        }

        private async Task CopyFolderAsync(string sourceFolder, string destinationFolder, CancellationToken token)
        {
            // Ensure source folder exists
            if (!Directory.Exists(sourceFolder))
            {
                throw new DirectoryNotFoundException($"Source folder not found: {sourceFolder}");
            }

            // Create destination folder if it doesn't exist
            if (!Directory.Exists(destinationFolder))
            {
                Directory.CreateDirectory(destinationFolder);
            }

            // Copy all files
            foreach (string file in Directory.GetFiles(sourceFolder))
            {
                string fileName = Path.GetFileName(file);
                string destFilePath = Path.Combine(destinationFolder, fileName);
                await CopyFileAsync(file, destFilePath, token);
            }

            // Recursively copy all subfolders
            foreach (string subFolder in Directory.GetDirectories(sourceFolder))
            {
                string folderName = Path.GetFileName(subFolder);
                string destSubFolder = Path.Combine(destinationFolder, folderName);
                await CopyFolderAsync(subFolder, destSubFolder, token);
            }
        }

        private async Task CopyFileAsync(string sourceFile, string destinationFile, CancellationToken token)
        {
            using var sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
            using var destinationStream = new FileStream(destinationFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
            logger.LogInformation("Copy files : [{from}] -> [{to}]", sourceFile, destinationFile);
            await sourceStream.CopyToAsync(destinationStream, token);
        }
    }
}
