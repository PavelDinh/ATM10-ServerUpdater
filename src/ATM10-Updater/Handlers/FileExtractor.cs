using ATM10Updater.Providers;
using Microsoft.Extensions.Logging;
using System.IO.Compression;

namespace ATM10Updater.Handlers
{
    public class FileExtractor(ILogger<FileExtractor> logger, 
        IServerVersionProvider versionProvider)
        : IFileExtractor
    {
        /// <summary>
        /// Extracts a ZIP file to the specified destination folder.
        /// </summary>
        /// <param name="zipFilePath">The full path to the ZIP file.</param>
        /// <param name="destinationFolder">The directory where the contents will be extracted.</param>
        /// <param name="overwrite">If true, existing files in the destination will be overwritten. Default is false.</param>
        public void ExtractZipFile(string zipFilePath, string destinationFolder, bool overwrite = false)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(zipFilePath))
                throw new ArgumentException("ZIP file path cannot be null or empty.", nameof(zipFilePath));

            if (string.IsNullOrWhiteSpace(destinationFolder))
                throw new ArgumentException("Destination folder cannot be null or empty.", nameof(destinationFolder));

            if (!File.Exists(zipFilePath))
                throw new FileNotFoundException($"The ZIP file '{zipFilePath}' does not exist.");

            logger.LogInformation("Extracting ZIP file {ZipFilePath} to {DestinationFolder}", zipFilePath, destinationFolder);

            try
            {
                // Extract the ZIP file
                ZipFile.ExtractToDirectory(zipFilePath, destinationFolder, overwrite);

                logger.LogInformation("Extraction completed successfully for ZIP file {ZipFilePath}", zipFilePath);
            }
            catch (IOException ex)
            {
                logger.LogError(ex, "An error occurred while extracting the ZIP file {ZipFilePath} to {DestinationFolder}", zipFilePath, destinationFolder);
                throw;
            }
        }

        /// <summary>
        /// Renames a folder to a new name.
        /// </summary>
        /// <param name="targetFolder">The full path of the folder to rename.</param>
        /// <param name="newName">The new name for the folder.</param>
        /// <param name="overwrite">If true, overwrites an existing folder with the same name. Default is false.</param>
        public void RenameFolder(string targetFolder, string newName, bool overwrite = false)
        {
            if (string.IsNullOrWhiteSpace(targetFolder))
                throw new ArgumentException("The target folder path cannot be null or empty.", nameof(targetFolder));

            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("The new folder name cannot be null or empty.", nameof(newName));

            if (!Directory.Exists(targetFolder))
                throw new DirectoryNotFoundException($"The folder '{targetFolder}' does not exist.");

            // Get the parent directory and new folder path
            string parentDirectory = Path.GetDirectoryName(targetFolder) ?? throw new InvalidOperationException("The target folder must have a valid parent directory.");
            string newFolderPath = Path.Combine(parentDirectory, newName);

            // Validate the new folder name
            if (newName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                throw new ArgumentException($"The new name '{newName}' contains invalid characters.", nameof(newName));

            // Check for overwrite condition
            if (Directory.Exists(newFolderPath))
            {
                if (overwrite)
                {
                    Directory.Delete(newFolderPath, true); // Delete existing folder if overwrite is enabled
                    logger.LogInformation("Existing folder [{newFolderPath}] was overwritten.", newFolderPath);
                }
                else
                {
                    throw new IOException($"A folder with the name '{newName}' already exists in '{parentDirectory}'.");
                }
            }

            // Perform the rename
            Directory.Move(targetFolder, newFolderPath);
            logger.LogInformation("Folder renamed from [{targetFolder}] to [{newFolderPath}].", targetFolder, newFolderPath);
        }

        public static string ShouldExtractIntoFolder(string zipFile, string rootDirectory, string targetDirectory)
        {
            using ZipArchive archive = ZipFile.OpenRead(zipFile);
            if (archive.Entries.Count > 1)
            {
                return targetDirectory;
            }

            return rootDirectory;
        }

        public string DecideExtractFolderTarget(string zipFile, string defaultFolder)
        {
            try
            {
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(zipFile);
                using ZipArchive archive = ZipFile.OpenRead(zipFile);
                if (archive.Entries[0].FullName.Contains(fileNameWithoutExtension))
                {
                    return defaultFolder;
                }

                return Path.Combine(Path.GetDirectoryName(zipFile)!, Path.GetFileNameWithoutExtension(zipFile));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task ExtractAndRenameServerFolder(string downloadFilePath, string localServerFolder, string namingConvention)
        {
            await Task.Run(() =>
            {
                var extractFolder = DecideExtractFolderTarget(downloadFilePath, localServerFolder);

                ExtractZipFile(downloadFilePath, extractFolder);

                var latestVersionFolderName = namingConvention + versionProvider.GetLatestVersion()?.ToString()!;
                var extractedTargetFolder = Path.Combine(Path.GetDirectoryName(downloadFilePath)!, Path.GetFileNameWithoutExtension(downloadFilePath));
                RenameFolder(extractedTargetFolder, latestVersionFolderName, true);

                File.Delete(downloadFilePath);
            });
        }
    }
}
