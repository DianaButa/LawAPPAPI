using LawProject.Database;
using LawProject.DTO;
using LawProject.Models;
using Microsoft.EntityFrameworkCore;

namespace LawProject.Service.TaskService
{
  public class TaskService :ITaskService 
  {

    private readonly ApplicationDbContext _context;
    private readonly ILogger<TaskService> _logger;

    public TaskService(ApplicationDbContext context, ILogger<TaskService> logger)
    {
      _context = context;
      _logger = logger;
    }

    public async Task<WorkTask> CreateTaskAsync(CreateTaskDto dto)
    {
      // Verificăm existența avocatului
      var lawyer = await _context.Lawyers.FirstOrDefaultAsync(l => l.Id == dto.LawyerId);
      if (lawyer == null)
      {
        _logger.LogWarning($"Avocatul cu ID {dto.LawyerId} nu a fost găsit.");
        throw new ArgumentException($"Avocatul cu ID {dto.LawyerId} nu a fost găsit.");
      }

      // Validăm existența clientului în funcție de tipul acestuia
      if (dto.ClientType.ToUpper() == "PF")
      {
        var clientPF = await _context.ClientPFs.FirstOrDefaultAsync(c => c.Id == dto.ClientId);
        if (clientPF == null)
        {
          _logger.LogWarning($"Clientul fizic cu ID {dto.ClientId} nu a fost găsit.");
          throw new ArgumentException($"Clientul fizic cu ID {dto.ClientId} nu a fost găsit.");
        }
      }
      else if (dto.ClientType.ToUpper() == "PJ")
      {
        var clientPJ = await _context.ClientPJs.FirstOrDefaultAsync(c => c.Id == dto.ClientId);
        if (clientPJ == null)
        {
          _logger.LogWarning($"Clientul juridic cu ID {dto.ClientId} nu a fost găsit.");
          throw new ArgumentException($"Clientul juridic cu ID {dto.ClientId} nu a fost găsit.");
        }
      }
      else
      {
        _logger.LogWarning($"ClientType invalid: {dto.ClientType}");
        throw new ArgumentException($"ClientType invalid: {dto.ClientType}. Se acceptă doar PF sau PJ.");
      }

      var workTask = new WorkTask
      {
        LawyerId = dto.LawyerId,
        StartDate = dto.StartDate,
        EndDate = dto.EndDate,
        Status = "open",  // status implicit la crearea task‑ului
        ClientId = dto.ClientId,
        ClientType = dto.ClientType.ToUpper(),
        Title = dto.Title,
        Description = dto.Description,
        FileId = dto.FileId,
        FileNumber = dto.FileNumber,
        Comment= dto.Comment,
      };

      _context.Tasks.Add(workTask);
      await _context.SaveChangesAsync();

      _logger.LogInformation($"Task creat cu ID {workTask.Id} pentru clientul {dto.ClientType} cu ID {dto.ClientId}.");
      return workTask;
    }

    public async Task<WorkTask> CloseTaskAsync(int taskId, CloseTaskDto dto)
    {
      var workTask = await _context.Tasks.FindAsync(taskId);
      if (workTask == null)
      {
        throw new ArgumentException("Taskul nu a fost găsit.", nameof(taskId));
      }

      if (workTask.Status.ToLower() == "closed")
      {
        throw new InvalidOperationException("Taskul este deja închis.");
      }

      workTask.Status = "closed";
      workTask.HoursWorked = dto.HoursWorked;

   

      _context.Tasks.Update(workTask);
      await _context.SaveChangesAsync();

      _logger.LogInformation($"Taskul cu ID {taskId} a fost închis. Ore lucrate: {dto.HoursWorked}.");
      return workTask;
    }


    public async Task<IEnumerable<WorkTask>> GetAllTasksAsync()
    {
      return await _context.Tasks.ToListAsync();
    }

    public async Task<IEnumerable<WorkTask>> GetTasksByLawyerIdAsync(int lawyerId)
    {
      return await _context.Tasks
          .Where(t => t.LawyerId == lawyerId)
          .ToListAsync();
    }
  }
}

