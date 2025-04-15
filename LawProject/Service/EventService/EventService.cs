using LawProject.Database;
using LawProject.DTO;
using LawProject.Models;
using Microsoft.EntityFrameworkCore;

namespace LawProject.Service.EventService
{
  public class EventService : IEventService
  {
    private readonly ApplicationDbContext _dbContext;



    public EventService(ApplicationDbContext dbContext)
    {
      _dbContext = dbContext;
    }

    // Adaugă un eveniment audiere
    public async Task<EventADTO> AddEventAAsync(EventADTO eventADto)
    {
      // Verifică dacă Time este valid
      if (!TimeSpan.TryParse(eventADto.Time.ToString(), out TimeSpan parsedTime))
      {
        throw new ArgumentException("Invalid time format. Expected format: HH:mm");
      }

      // Crează entitatea EventA și salvează Time ca TimeSpan
      var eventEntity = new EventA
      {
        Date = eventADto.Date,
        Time = parsedTime,  // Salvează ca TimeSpan, nu ca string
        Description = eventADto.Description
      };

      _dbContext.EventsA.Add(eventEntity);
      await _dbContext.SaveChangesAsync();

      // Atribuie Time ca TimeSpan, fără a-l transforma într-un string
      eventADto.Id = eventEntity.Id;
      eventADto.Time = eventEntity.Time;  // Păstrează TimeSpan, nu convertește în string
      return eventADto;
    }




    // Adaugă un eveniment consultanta
    public async Task<EventCDTO> AddEventCAsync(EventCDTO eventCDto)
    {
      // Înlocuim secunda cu 00, dacă nu există
      if (!TimeSpan.TryParse(eventCDto.Time + ":00", out TimeSpan parsedTime))
      {
        throw new ArgumentException("Invalid time format. Expected format: HH:mm");
      }

      var eventEntity = new EventC
      {
        Date = eventCDto.Date,
        Time = parsedTime,
        Description = eventCDto.Description
      };

      _dbContext.EventsC.Add(eventEntity);
      await _dbContext.SaveChangesAsync();

      eventCDto.Id = eventEntity.Id;
      eventCDto.Time = eventEntity.Time;  // Reformatăm ora ca HH:mm
      return eventCDto;
    }


    // Obține toate evenimentele A din baza de date
    public async Task<List<EventADTO>> GetAllEventsAAsync()
    {
      var events = await _dbContext.EventsA.ToListAsync();
      return events.Select(e => new EventADTO
      {
        Id = e.Id,
        Date = e.Date,
        Time = e.Time,
        Description = e.Description
      }).ToList();
    }

    // Obține toate evenimentele C din baza de date
    public async Task<List<EventCDTO>> GetAllEventsCAsync()
    {
      var events = await _dbContext.EventsC.ToListAsync();
      return events.Select(e => new EventCDTO
      {
        Id = e.Id,
        Date = e.Date,
        Time = e.Time,
        Description = e.Description
      }).ToList();
    }
  }
}

