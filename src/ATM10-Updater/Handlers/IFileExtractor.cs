namespace ATM10Updater.Handlers
{
    public interface IFileExtractor
    {
        void ExtractZipFile(string zipFilePath, string destinationFolder, bool overwrite = false);

        void RenameFolder(string targetFolder, string newName, bool overwrite = false);

        string DecideExtractFolderTarget(string zipFile, string defaultFolder);
    }
}