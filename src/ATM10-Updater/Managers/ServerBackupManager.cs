using ATM10Updater.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ATM10Updater.Managers
{
    public class ServerBackupManager(ILogger<ServerBackupManager> logger,
        IOptions<ServerConfig> serverInfo)
        : IServerBackupManager
    {
        public async Task LoadBackupAsync()
        {
            var latestServerFolder = Environment.GetEnvironmentVariable(serverInfo.Value.ServerFileEnv, EnvironmentVariableTarget.User)!;
            var olderServerFolders = Directory.GetDirectories(serverInfo.Value.LocalServerFolder, $"{serverInfo.Value.NamingConvention}*");
            olderServerFolders = olderServerFolders.OrderByDescending(x => Version.Parse(x.Split('-').Last())).Skip(1).ToArray();

            if (olderServerFolders == null || olderServerFolders.Length == 0)
            {
                // Accept eula
                var eulaTxtPath = $"{latestServerFolder}\\eula.txt";
                string eulaContent = await File.ReadAllTextAsync(eulaTxtPath);
                var acceptedEula = eulaContent.Replace("eula=false", "eula=true");
                await File.WriteAllTextAsync(eulaTxtPath, acceptedEula);

                return;
            }

            var olderServerFolder = olderServerFolders.ElementAt(0);
            foreach (var content in serverInfo.Value.BackupFiles)
            {
                var oldFile = Path.Combine(olderServerFolder, content);
                var newDestFile = Path.Combine(latestServerFolder, content);
                try
                {
                    if (File.GetAttributes(oldFile).HasFlag(FileAttributes.Directory))
                    {
                        if (string.IsNullOrWhiteSpace(oldFile))
                        {
                            logger.LogWarning("Folder was not present in Path : [{oldFIle}]", oldFile);
                            continue;
                        }

                        await CopyFolderAsync(oldFile, newDestFile);
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(oldFile))
                        {
                            logger.LogWarning("File was not present in Path : [{oldFIle}]", oldFile);
                            continue;
                        }

                        await CopyFileAsync(oldFile, newDestFile);
                    }

                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }

        private async Task CopyFolderAsync(string sourceFolder, string destinationFolder)
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
                await CopyFileAsync(file, destFilePath);
            }

            // Recursively copy all subfolders
            foreach (string subFolder in Directory.GetDirectories(sourceFolder))
            {
                string folderName = Path.GetFileName(subFolder);
                string destSubFolder = Path.Combine(destinationFolder, folderName);
                await CopyFolderAsync(subFolder, destSubFolder);
            }
        }

        private async Task CopyFileAsync(string sourceFile, string destinationFile)
        {
            using var sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
            using var destinationStream = new FileStream(destinationFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
            logger.LogInformation("Copy files : [{from}] -> [{to}]", sourceFile, destinationFile);
            await sourceStream.CopyToAsync(destinationStream);
        }
    }
}
