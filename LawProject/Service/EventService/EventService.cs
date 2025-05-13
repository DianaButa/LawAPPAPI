using LawProject.Database;
using LawProject.DTO;
using LawProject.Models;
using Microsoft.EntityFrameworkCore;
using System.Drawing;

namespace LawProject.Service.EventService
{
  public class EventService : IEventService
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<EventService> _logger;



    public EventService(ApplicationDbContext dbContext, ILogger<EventService> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    private async Task<string> GetClientNameAsync(string clientType, int clientId)
    {
      return clientType.ToUpper() switch
      {
        "PF" => await _dbContext.ClientPFs
                    .Where(c => c.Id == clientId)
                    .Select(c => c.FirstName + " " + c.LastName)
                    .FirstOrDefaultAsync() ?? "Necunoscut",

        "PJ" => await _dbContext.ClientPJs
                    .Where(c => c.Id == clientId)
                    .Select(c => c.CompanyName)
                    .FirstOrDefaultAsync() ?? "Necunoscut",

        _ => "Necunoscut"
      };
    }

    private async Task<(string? name, string color)> GetLawyerInfoAsync(int lawyerId)
    {
      var lawyer = await _dbContext.Lawyers.FindAsync(lawyerId);
      if (lawyer == null)
        return ("Necunoscut", "#E0E0E0");

      return (lawyer.LawyerName, string.IsNullOrWhiteSpace(lawyer.Color) ? "#E0E0E0" : lawyer.Color);
    }

    public async Task<EventADTO> AddEventAAsync(EventADTO dto)
    {
      if (dto.Date <= DateTime.MinValue)
        throw new ArgumentException("Invalid date.");

      TimeSpan parsedTime;
      var timeString = dto.Time?.ToString()?.Trim();

      if (string.IsNullOrEmpty(timeString))
        throw new ArgumentException("Time is required.");

      if (TimeSpan.TryParseExact(timeString, new[] { @"h", @"hh", @"h\:mm", @"hh\:mm" }, null, out parsedTime))
      {
        // valid format, do nothing
      }
      else
      {
        throw new ArgumentException("Invalid time format. Expected format: HH or HH:mm");
      }


      var (lawyerName, color) = await GetLawyerInfoAsync(dto.LawyerId);
      var clientName = await GetClientNameAsync(dto.ClientType, dto.ClientId);

      var entity = new EventA
      {
        Date = dto.Date,
        Time = parsedTime,
        Description = dto.Description,
        ClientId = dto.ClientId,
        ClientType = dto.ClientType.ToUpper(),
        FileId = dto.FileId,
        FileNumber = dto.FileNumber,
        LawyerId = dto.LawyerId,
        Color = color
      };

      _dbContext.EventsA.Add(entity);
      await _dbContext.SaveChangesAsync();

      return new EventADTO
      {
        Id = entity.Id,
        Date = entity.Date,
        Time = entity.Time.ToString(@"hh\:mm"),
        Description = entity.Description,
        ClientId = entity.ClientId,
        ClientType = entity.ClientType,
        ClientName = clientName,
        FileId = entity.FileId,
        FileNumber = entity.FileNumber,
        LawyerId = entity.LawyerId,
        LawyerName = lawyerName,
        Color = color
      };
    }

    public async Task<EventCDTO> AddEventCAsync(EventCDTO dto)
    {
      if (dto.Date <= DateTime.MinValue)
        throw new ArgumentException("Invalid date.");

      TimeSpan parsedTime;
      var timeString = dto.Time?.ToString()?.Trim();

      if (string.IsNullOrEmpty(timeString))
        throw new ArgumentException("Time is required.");

      if (TimeSpan.TryParseExact(timeString, new[] { @"h", @"hh", @"h\:mm", @"hh\:mm" }, null, out parsedTime))
      {
        // valid format, do nothing
      }
      else
      {
        throw new ArgumentException("Invalid time format. Expected format: HH or HH:mm");
      }


      var (lawyerName, color) = await GetLawyerInfoAsync(dto.LawyerId);
      var clientName = await GetClientNameAsync(dto.ClientType, dto.ClientId);

      var entity = new EventC
      {
        Date = dto.Date,
        Time = parsedTime,
        Description = dto.Description,
        ClientId = dto.ClientId,
        ClientType = dto.ClientType.ToUpper(),
        FileId = dto.FileId,
        FileNumber = dto.FileNumber,
        LawyerId = dto.LawyerId,
        IsReported= dto.IsReported,
        Color = color
      };

      _dbContext.EventsC.Add(entity);
      await _dbContext.SaveChangesAsync();

      return new EventCDTO
      {
        Id = entity.Id,
        Date = entity.Date,
        Time = entity.Time.ToString(@"hh\:mm"),
        Description = entity.Description,
        ClientId = entity.ClientId,
        ClientType = entity.ClientType,
        ClientName = clientName,
        FileId = entity.FileId,
        FileNumber = entity.FileNumber,
        IsReported = entity.IsReported,
        LawyerId = entity.LawyerId,
        LawyerName = lawyerName,
        Color = color
      };
    }

    public async Task<List<EventADTO>> GetAllEventsAAsync()
    {
      var events = await _dbContext.EventsA.Include(e => e.Lawyer).ToListAsync();

      var result = new List<EventADTO>();

      foreach (var e in events)
      {
        var clientName = await GetClientNameAsync(e.ClientType, e.ClientId);

        result.Add(new EventADTO
        {
          Id = e.Id,
          Date = e.Date,
          Time = e.Time.ToString(@"hh\:mm"),
          Description = e.Description,
          ClientId = e.ClientId,
          ClientType = e.ClientType,
          ClientName = clientName,
          FileId = e.FileId,
          IsReported =e.IsReported,
          FileNumber = e.FileNumber,
          LawyerId = e.LawyerId,
          LawyerName = e.Lawyer?.LawyerName ?? "Necunoscut",
          Color = !string.IsNullOrEmpty(e.Lawyer?.Color) ? e.Lawyer.Color : "#E0E0E0"
        });
      }

      return result;
    }

    public async Task<List<EventCDTO>> GetAllEventsCAsync()
    {
      var events = await _dbContext.EventsC.Include(e => e.Lawyer).ToListAsync();

      var result = new List<EventCDTO>();

      foreach (var e in events)
      {
        var clientName = await GetClientNameAsync(e.ClientType, e.ClientId);

        result.Add(new EventCDTO
        {
          Id = e.Id,
          Date = e.Date,
          Time = e.Time.ToString(@"hh\:mm"),
          Description = e.Description,
          ClientId = e.ClientId,
          ClientType = e.ClientType,
          ClientName = clientName,
          FileId = e.FileId,
          IsReported = e.IsReported,
          FileNumber = e.FileNumber,
          LawyerId = e.LawyerId,
          LawyerName = e.Lawyer?.LawyerName ?? "Necunoscut",
          Color = !string.IsNullOrEmpty(e.Lawyer?.Color) ? e.Lawyer.Color : "#E0E0E0"
        });
      }

      return result;
    }
  }
}
