using LawProject.Database;
using LawProject.DTO;
using LawProject.Models;
using Microsoft.EntityFrameworkCore;
using System.ServiceModel;

namespace LawProject.Service.DailyEventService
{
  public class DailyEventService :IDailyEventService
  {
    private readonly ApplicationDbContext _context;

    public DailyEventService(ApplicationDbContext context)
    {
      _context = context;
    }

    public async Task AddDailyEventAsync(DailyEventsDto Dto)
    {
      var newDailyEvent = new DailyEvents
      {
        FileNumber = Dto.FileNumber,
        Date = Dto.Date,
        Institutie = Dto.Institutie,
        ClientName = Dto.ClientName,
        ClientType = Dto.ClientType,
        ClientId = Dto.ClientId,
        LawyerId = Dto.LawyerId,
        LawyerName=Dto.LawyerName,
        Descriere = Dto.Descriere,
        AllocatedHours = Dto.AllocatedHours
      };

      
      if (Dto.EventType == "S") 
      {
        var scheduledEvent = await _context.ScheduledEvents
            .FirstOrDefaultAsync(e => e.Id == Dto.Id);  

        if (scheduledEvent != null)
        {
          scheduledEvent.IsReported = true; 
          _context.ScheduledEvents.Update(scheduledEvent);  
          newDailyEvent.ScheduledEventId = scheduledEvent.Id; 
        }
        else
        {
          throw new Exception($"Scheduled event with ID {Dto.Id} not found.");
        }
      }
      else if (Dto.EventType == "A")
      {
        var eventA = await _context.EventsA
            .FirstOrDefaultAsync(e => e.Id == Dto.Id);  

        if (eventA != null)
        {
          eventA.IsReported = true;  
          _context.EventsA.Update(eventA);  
          newDailyEvent.EventAId = eventA.Id; 
        }
        else
        {
          throw new Exception($"Event A with ID {Dto.Id} not found.");
        }
      }
      else if (Dto.EventType == "C")
      {
        var eventC = await _context.EventsC
            .FirstOrDefaultAsync(e => e.Id == Dto.Id);  

        if (eventC != null)
        {
          eventC.IsReported = true; 
          _context.EventsC.Update(eventC);  
          newDailyEvent.EventCId = eventC.Id; 
        }
        else
        {
          throw new Exception($"Event C with ID {Dto.Id} not found.");
        }
      }
      else
      {
    
        throw new ArgumentException("Invalid event type.");
      }

   
      _context.DailyEvents.Add(newDailyEvent);
      await _context.SaveChangesAsync();  
    }


    public async Task<List<DailyEventsDto>> GetAllDailyEventsAsync()
    {
      return await _context.DailyEvents
        .Include(e => e.Lawyer)
        .Select(e => new DailyEventsDto
        {
          Id = e.Id,
          FileNumber = e.FileNumber,
          Date = e.Date,
          Institutie = e.Institutie,
          Descriere = e.Descriere,
          ClientName = e.ClientName,
          ClientId = e.ClientId,
          ClientType = e.ClientType,
          LawyerId = e.LawyerId,
          LawyerName = e.Lawyer.LawyerName,
          AllocatedHours = e.AllocatedHours
        })
        .ToListAsync();
    }

    public async Task<List<DailyEventsDto>> GetEventsByFileNumber(string fileNumber)
    {
      return await _context.DailyEvents
      .Include(e => e.Lawyer)
         .Where(e => e.FileNumber.ToLower().Contains(fileNumber.ToLower()))
        .Select(e => new DailyEventsDto
        {
          Id = e.Id,
          FileNumber = e.FileNumber,
          Date = e.Date,
          Institutie = e.Institutie,
          Descriere = e.Descriere,
          ClientName = e.ClientName,
          ClientId = e.ClientId,
          ClientType = e.ClientType,
          LawyerId = e.LawyerId,
          LawyerName = e.Lawyer.LawyerName,
          AllocatedHours = e.AllocatedHours
        })
        .ToListAsync();
    }

    public async Task<IEnumerable<DailyEventsDto>> GetEventsByClient(string clientName )
    {
      return await _context.DailyEvents
    .Include(e => e.Lawyer)
    .Where(e => e.ClientName.ToLower().Contains(clientName.ToLower()))
    .Select(e => new DailyEventsDto
    {
      Id = e.Id,
      FileNumber = e.FileNumber,
      Date = e.Date,
      Institutie = e.Institutie,
      Descriere = e.Descriere,
      ClientName = e.ClientName,
      ClientId = e.ClientId,
      ClientType = e.ClientType,
      LawyerId = e.LawyerId,
      LawyerName = e.Lawyer.LawyerName,
      AllocatedHours = e.AllocatedHours
    })
    .ToListAsync();
    }


    // Get daily events by lawyerId
    public async Task<List<DailyEventsDto>> GetDailyEventsByLawyerIdAsync(int lawyerId)
    {
      return await _context.DailyEvents
        .Where(e => e.LawyerId == lawyerId)
        .Select(e => new DailyEventsDto
        {
          Id = e.Id,
          FileNumber = e.FileNumber,
          Date = e.Date,
          Descriere = e.Descriere,
          Institutie = e.Institutie,
          ClientName = e.ClientName,
          ClientType=e.ClientType,
          ClientId=e.ClientId,
          LawyerId = e.LawyerId,
          LawyerName=e.LawyerName,
          AllocatedHours = e.AllocatedHours
        })
        .ToListAsync();
    }

  }
}
