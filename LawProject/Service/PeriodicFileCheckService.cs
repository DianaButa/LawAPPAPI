using LawProject.Service.FileService;

namespace LawProject.Service
{
  public class PeriodicFileCheckService : BackgroundService
  {
    private readonly FileToCalendarService _fileToCalendarService;
    private readonly ILogger<PeriodicFileCheckService> _logger;

    public PeriodicFileCheckService(FileToCalendarService fileToCalendarService, ILogger<PeriodicFileCheckService> logger)
    {
      _fileToCalendarService = fileToCalendarService;
      _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      while (!stoppingToken.IsCancellationRequested)
      {
        _logger.LogInformation("Starting periodic file check...");

        try
        {
          await _fileToCalendarService.ProcessAllFilesAsync();
        }
        catch (Exception ex)
        {
          _logger.LogError($"Error during file check: {ex.Message}");
        }

        // Așteaptă 5 minute înainte de următoarea verificare
        await Task.Delay(TimeSpan.FromMinutes(50), stoppingToken);
      }
    }
  }
}
