using LawProject.Database;
using LawProject.DTO;
using LawProject.Models;
using Microsoft.EntityFrameworkCore;

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
        Date = DateTime.UtcNow,
        Institutie = Dto.Institutie,
        ClientName = Dto.ClientName,
        LawyerId = Dto.LawyerId,
        Descriere = Dto.Descriere,
        AllocatedHours = Dto.AllocatedHours
      };

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
          LawyerId = e.LawyerId,
          AllocatedHours = e.AllocatedHours
        })
        .ToListAsync();
    }

  }
}
