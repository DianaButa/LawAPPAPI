using LawProject.Service.FileService;

namespace LawProject.Service
{
  public class BackgroundFileCacheUpdater
  {

    private readonly IFileManagementService _fileService;
    private readonly ILogger<BackgroundFileCacheUpdater> _logger;

    public BackgroundFileCacheUpdater(IFileManagementService fileService, ILogger<BackgroundFileCacheUpdater> logger)
    {
      _fileService = fileService;
      _logger = logger;
    }

    public async Task RefreshCacheJob()
    {
      _logger.LogInformation("Pornire job Hangfire pentru actualizarea cache-ului dosarelor.");
      await _fileService.RefreshCacheAsync();
      _logger.LogInformation("Job Hangfire finalizat.");
    }
  }

}
