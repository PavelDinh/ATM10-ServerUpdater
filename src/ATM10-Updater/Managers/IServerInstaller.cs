namespace ATM10Updater.Managers
{
    public interface IServerInstaller
    {
        /// <summary>
        /// Downloads latest version of the modpack server files
        /// </summary>
        /// <returns>Path where it was downloaded</returns>
        Task<string> InstallAsync();

        bool IsNewVersionAvailable();
    }
}
