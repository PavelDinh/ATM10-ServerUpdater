namespace CurseForgeAPI
{
    public interface ICurseForgeClient
    {
        public Task<string> GetModAsync(int modId);

        public Task<string> GetModFilesAsync(int modId);

        public Task<string> GetDownloadFileAsync(int modId, int fileId);

        public Task<string> GetModFileChangelogAsync(int modId, int fileId);
    }
}