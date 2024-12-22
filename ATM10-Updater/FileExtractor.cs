using System.IO.Compression;

namespace ATM10Updater
{
    public static class FileExtractor
    {
        public static void ExtractAndOrganize(string zipFilePath, string destinationFolder, string namingConvention, Version latestVersion)
        {
            var tempFolder = Path.Combine(destinationFolder, "temp");
            ZipFile.ExtractToDirectory(zipFilePath, tempFolder);

            var extractedFolder = Directory.GetDirectories(tempFolder).First();
            var newFolderName = $"{namingConvention}{latestVersion}";
            var newFolderPath = Path.Combine(destinationFolder, newFolderName);

            Directory.Move(extractedFolder, newFolderPath);
            Directory.Delete(tempFolder, true);
            File.Delete(zipFilePath);
        }
    }
}
