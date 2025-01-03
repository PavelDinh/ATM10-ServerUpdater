using CurseForgeAPI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CurseForgeWebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CurseForgeApiController(ICurseForgeClient curseForgeClient) : ControllerBase
    {
        [HttpGet("mod/{modId}")]
        public async Task<IActionResult> GetModInfo(int modId)
        {
            try
            {
                var modInfo = await curseForgeClient.GetModAsync(modId);
                return Content(modInfo, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching mod info: {ex.Message}");
            }
        }

        [HttpGet("mod/{modId}/files")]
        public async Task<IActionResult> GetModFiles(int modId)
        {
            try
            {
                var files = await curseForgeClient.GetModFilesAsync(modId);
                return Content(files, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching mod files: {ex.Message}");
            }
        }

        [HttpGet("mod/{modId}/file/{fileId}/changelog")]
        public async Task<IActionResult> GetModFileChangelog(int modId, int fileId)
        {
            try
            {
                var changelog = await curseForgeClient.GetModFileChangelogAsync(modId, fileId);
                return Content(changelog, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching file changelog: {ex.Message}");
            }
        }

        [HttpGet("mod/{modId}/file/{fileId}/download-url")]
        public async Task<IActionResult> GetFileDownloadUrl(int modId, int fileId)
        {
            try
            {
                var downloadUrl = await curseForgeClient.GetDownloadFileAsync(modId, fileId);
                return Content(downloadUrl, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching file download URL: {ex.Message}");
            }
        }
    }
}
