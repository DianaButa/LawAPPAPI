using LawProject.Database;
using LawProject.DTO;
using LawProject.Models;
using LawProject.Service.DailyEventService;
using LawProject.Service.FileService;
using LawProject.Service.RaportService;
using LawProject.Service.TaskService;
using Microsoft.EntityFrameworkCore;

namespace LawProject.Service.Lawyer
{
  public class LawyerService : ILawyerService
  {
    private readonly ApplicationDbContext _context;
    private readonly IFileManagementService _fileService;
    private readonly ITaskService _taskService;
    private readonly IRaportService _raportService;
    private readonly IDailyEventService _dailyEventService;

    public LawyerService(ApplicationDbContext context, IFileManagementService fileService,
        ITaskService taskService, IRaportService raportService, IDailyEventService dailyEventService )
    {
      _context = context;
      _fileService = fileService;
      _taskService = taskService;
      _raportService = raportService;
      _dailyEventService = dailyEventService;
    }

    public async Task<IEnumerable<LawyerDto>> GetAllLawyersAsync()
    {
      var lawyers = await _context.Lawyers
          .Select(l => new LawyerDto
          {
            Id = l.Id,
            LawyerName = l.LawyerName,
            Color = l.Color 
          })
          .ToListAsync();

      return lawyers;
    }


    public async Task<LawyerDto> AddLawyerAsync(LawyerDto lawyerDto)
    {
      // Validăm culoarea avocatului (dacă vrei să o validezi ca fiind un cod de culoare valid)
      if (string.IsNullOrEmpty(lawyerDto.Color))
      {
        throw new ArgumentException("Culoarea avocatului este obligatorie.");
      }

      var lawyer = new Models.Lawyer
      {
        LawyerName = lawyerDto.LawyerName,
        Color = lawyerDto.Color
      };

      _context.Lawyers.Add(lawyer);
      await _context.SaveChangesAsync();

      lawyerDto.Id = lawyer.Id;
      return lawyerDto;
    }

    public async Task<LawyerOverviewDto> GetOverviewByLawyerIdAsync(int lawyerId)
    {
      var lawyer = await _context.Lawyers.FindAsync(lawyerId);
      if (lawyer == null)
      {
        throw new KeyNotFoundException($"Avocatul cu ID {lawyerId} nu există.");
      }

      var openFiles = await _fileService.GetOpenFilesByLawyerIdAsync(lawyerId);
      var closedFiles = await _fileService.GetClosedFilesByLawyerIdAsync(lawyerId);
      var openTasks = await _taskService.GetTasksByLawyerIdAndOpenStatusAsync(lawyerId);
      var closedTasks = await _taskService.GetTasksByLawyerIdAndClosedStatusAsync(lawyerId);

      return new LawyerOverviewDto
      {
        LawyerId = lawyer.Id,
        LawyerName = lawyer.LawyerName,
        OpenFilesCount = openFiles.Count,
        ClosedFilesCount = closedFiles.Count,
        OpenTasksCount = openTasks.Count,
        ClosedTasksCount = closedTasks.Count,
        OpenFiles = openFiles,
        ClosedFiles = closedFiles,
        OpenTasks = openTasks,
        ClosedTasks = closedTasks

      };
    }
    public async Task<List<LawyerOverviewDto>> GetAllLawyerOverviewsAsync()
    {
      var lawyers = await _context.Lawyers.ToListAsync();

      var lawyerOverviews = new List<LawyerOverviewDto>();

      foreach (var lawyer in lawyers)
      {
        var openFiles = await _fileService.GetOpenFilesByLawyerIdAsync(lawyer.Id);
        var closedFiles = await _fileService.GetClosedFilesByLawyerIdAsync(lawyer.Id);
        var openTasks = await _taskService.GetTasksByLawyerIdAndOpenStatusAsync(lawyer.Id);
        var closedTasks = await _taskService.GetTasksByLawyerIdAndClosedStatusAsync(lawyer.Id);

        lawyerOverviews.Add(new LawyerOverviewDto
        {
          LawyerId = lawyer.Id,
          LawyerName = lawyer.LawyerName,
          LawyerColor=lawyer.Color,
          OpenFilesCount = openFiles.Count,
          ClosedFilesCount = closedFiles.Count,
          OpenTasksCount = openTasks.Count,
          ClosedTasksCount = closedTasks.Count,
          OpenFiles = openFiles,
          ClosedFiles = closedFiles,
          OpenTasks = openTasks,
          ClosedTasks = closedTasks
        });
      }

      return lawyerOverviews;
    }

    public async Task<LawyerDashboardDto> GetLawyerDashboardDataAsync(int lawyerId, DateTime? startDate, DateTime? endDate)
    {
      var files = (await _fileService.GetFilesByLawyerIdAsync(lawyerId))?.ToList() ?? new();
      var openTasks = (await _taskService.GetTasksByLawyerIdAndOpenStatusAsync(lawyerId))?.ToList() ?? new();
      var closedTasks = (await _taskService.GetTasksByLawyerIdAndClosedStatusAsync(lawyerId))?.ToList() ?? new();
      var dailyEvents = (await _dailyEventService.GetDailyEventsByLawyerIdAsync(lawyerId))?.ToList() ?? new();
      var rapoarte = (await _raportService.GetRapoarteByLawyerIdAsync(lawyerId))?.ToList() ?? new();

      if (startDate.HasValue && endDate.HasValue)
      {
        dailyEvents = dailyEvents
          .Where(e => e.Date >= startDate.Value && e.Date <= endDate.Value)
          .ToList();

        openTasks = openTasks
          .Where(t => t.StartDate >= startDate.Value && t.StartDate <= endDate.Value)
          .ToList();

        closedTasks = closedTasks
          .Where(t => t.StartDate >= startDate.Value && t.StartDate <= endDate.Value)
          .ToList();

        rapoarte = rapoarte
          .Where(r => r.DataRaport >= startDate.Value && r.DataRaport <= endDate.Value)
          .ToList();
      }

      return new LawyerDashboardDto
      {
        LawyerId = lawyerId,
        Files = files,
        DailyEvents = dailyEvents,
        OpenTasks = openTasks,
        ClosedTasks = closedTasks,
        Raport = rapoarte
      };
    }




  }
}
