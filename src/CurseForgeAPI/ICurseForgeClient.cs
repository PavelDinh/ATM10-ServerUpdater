namespace CurseForgeAPI
{
    public interface ICurseForgeClient
    {
        public Task<string> GetModAsync(int modId, CancellationToken token);

        public Task<string> GetModFilesAsync(int modId, CancellationToken token);

        public Task<string> GetDownloadFileAsync(int modId, int fileId, CancellationToken token);

        public Task<string> GetModFileChangelogAsync(int modId, int fileId, CancellationToken token);
    }
}