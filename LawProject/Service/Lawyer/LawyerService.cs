using LawProject.Database;
using LawProject.DTO;
using LawProject.Models;
using LawProject.Service.FileService;
using LawProject.Service.TaskService;
using Microsoft.EntityFrameworkCore;

namespace LawProject.Service.Lawyer
{
  public class LawyerService : ILawyerService
  {
    private readonly ApplicationDbContext _context;
    private readonly IFileManagementService _fileService;
    private readonly ITaskService _taskService;

    public LawyerService(ApplicationDbContext context, IFileManagementService fileService,
        ITaskService taskService )
    {
      _context = context;
      _fileService = fileService;
      _taskService = taskService;
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



  }
}
