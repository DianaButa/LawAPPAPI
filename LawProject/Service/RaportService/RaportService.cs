using LawProject.Database;
using LawProject.DTO;
using LawProject.Migrations;
using LawProject.Models;
using LawProject.Service.DailyEventService;
using LawProject.Service.FileService;
using LawProject.Service.TaskService;
using Microsoft.EntityFrameworkCore;

namespace LawProject.Service.RaportService
{
  public class RaportService : IRaportService
  {
    private readonly ApplicationDbContext _context;
    private readonly IDailyEventService _dailyEventService;
    private readonly IFileManagementService _fileService;
    private readonly ITaskService _taskService;

    public RaportService(ApplicationDbContext context, IDailyEventService dailyEventService, ITaskService taskService, IFileManagementService fileManagementService)
    {
      _context = context;
      _dailyEventService = dailyEventService;
      _taskService = taskService;
      _fileService = fileManagementService;
    }

    public async Task<int> CreateRaportAsync(RaportCreateDto dto)
    {
      // Obține avocatul
      var lawyer = await _context.Lawyers.FirstOrDefaultAsync(l => l.Id == dto.LawyerId);
      if (lawyer == null)
        throw new ArgumentException($"Avocatul cu ID {dto.LawyerId} nu a fost găsit.");

      // Inițializare date client
      string clientName = null;

      if (dto.ClientId.HasValue && !string.IsNullOrEmpty(dto.ClientType))
      {
        var clientType = dto.ClientType.ToUpper();

        switch (clientType)
        {
          case "PF":
            var clientPF = await _context.ClientPFs.FirstOrDefaultAsync(c => c.Id == dto.ClientId.Value);
            if (clientPF == null)
              throw new ArgumentException($"Clientul fizic cu ID {dto.ClientId.Value} nu a fost găsit.");

            clientName = $"{clientPF.FirstName} {clientPF.LastName}";
            break;

          case "PJ":
            var clientPJ = await _context.ClientPJs.FirstOrDefaultAsync(c => c.Id == dto.ClientId.Value);
            if (clientPJ == null)
              throw new ArgumentException($"Clientul juridic cu ID {dto.ClientId.Value} nu a fost găsit.");

            clientName = clientPJ.CompanyName;
            break;

          default:
            throw new ArgumentException($"Tipul clientului este invalid: {dto.ClientType}. Se acceptă doar 'PF' sau 'PJ'.");
        }
      }
      var raport = new Raport
      {
        LawyerId = dto.LawyerId,
        LawyerName = lawyer.LawyerName, 
        DataRaport = dto.Date,
        ClientId = dto.ClientId,
        ClientType = dto.ClientType,
        ClientName = clientName,
        FileId = dto.FileId,
        FileNumber = dto.FileNumber,
        OreDeplasare = dto.OreDeplasare,
        OreStudiu = dto.OreStudiu,
        TaskuriLucrate = dto.Taskuri?.Select(t => new RaportTask
        {
          WorkTaskId = t.WorkTaskId,
          OreLucrate = t.OreLucrate
        }).ToList()
      };

      _context.Rapoarte.Add(raport);
      await _context.SaveChangesAsync();

      return raport.Id;
    }


    public async Task<List<Raport>> GetAllRapoarteAsync()
    {
      return await _context.Rapoarte
          .Include(r => r.TaskuriLucrate)
          .ThenInclude(rt => rt.WorkTask)
          .ToListAsync();
    }

    public async Task<List<Raport>> GetRapoarteByClientAsync(int clientId, string clientType)
    {
      return await _context.Rapoarte
          .Include(r => r.TaskuriLucrate)
          .ThenInclude(rt => rt.WorkTask)
          .Where(r => r.ClientId == clientId && r.ClientType == clientType)
          .ToListAsync();
    }


    public async Task<Raport?> GetRaportByIdAsync(int id)
    {
      return await _context.Rapoarte
          .Include(r => r.TaskuriLucrate)
          .ThenInclude(rt => rt.WorkTask)
          .FirstOrDefaultAsync(r => r.Id == id);
    }


    public async Task<Raport?> GetRaportByFileNumberAsync(string fileNumber)
    {
      return await _context.Rapoarte
          .Include(r => r.TaskuriLucrate)
          .ThenInclude(rt => rt.WorkTask)
          .FirstOrDefaultAsync(r => r.FileNumber == fileNumber);
    }

    public async Task<List<RaportGeneralDto>> GetRapoarteGeneraleAsync()
    {
      var dailyEvents = await _dailyEventService.GetAllDailyEventsAsync();
      var rapoarte = await GetAllRapoarteAsync();

      var dailyEventDtos = dailyEvents.Select(e => new RaportGeneralDto
      {
        TipRaport = "DailyEvent",
        DailyEventId = e.Id,
        FileNumber = e.FileNumber,
        Date = e.Date,
        Institutie = e.Institutie,
        Descriere = e.Descriere,
        ClientName = e.ClientName,
        ClientId = e.ClientId,
        ClientType=e.ClientType,
        LawyerId = e.LawyerId,
        LawyerName = e.LawyerName,
        AllocatedHours = e.AllocatedHours,
        IsCompleted = e.IsCompleted,
        EventType = e.EventType
      });

      var raportDtos = rapoarte.Select(r => new RaportGeneralDto
      {
        TipRaport = "Raport",
        RaportId = r.Id,
        ClientType = r.ClientType,
        ClientId = r.ClientId,
        LawyerId = r.LawyerId,
        Date = r.Date,
        LawyerName = r.LawyerName,
        OreDeplasare = r.OreDeplasare,
        OreStudiu = r.OreStudiu,
        TaskuriLucrate = r.TaskuriLucrate.Select(t => t.WorkTask.FileNumber).ToList(),
      });

      return dailyEventDtos.Concat(raportDtos).ToList();
    }

    public async Task<List<RaportGeneralDto>> GetRapoarteGeneraleByLawyerAsync(int lawyerId)
    {
      var dailyEvents = await _dailyEventService.GetAllDailyEventsAsync();
      var rapoarte = await GetAllRapoarteAsync();

      var dailyEventDtos = dailyEvents
        .Where(e => e.LawyerId == lawyerId)
        .Select(e => new RaportGeneralDto
        {
          TipRaport = "DailyEvent",
          DailyEventId = e.Id,
          FileNumber = e.FileNumber,
          Date = e.Date,
          Institutie = e.Institutie,
          Descriere = e.Descriere,
          ClientName = e.ClientName,
          ClientId = e.ClientId,
          ClientType = e.ClientType,
          LawyerId = e.LawyerId,
          LawyerName = e.LawyerName,
          AllocatedHours = e.AllocatedHours,
          IsCompleted = e.IsCompleted,
          EventType = e.EventType
        });

      var raportDtos = rapoarte
        .Where(r => r.LawyerId == lawyerId)
        .Select(r => new RaportGeneralDto
        {
          TipRaport = "Raport",
          RaportId = r.Id,
          ClientType = r.ClientType,
          ClientId = r.ClientId,
          LawyerId = r.LawyerId,
          Date = r.Date,
          LawyerName = r.LawyerName,
          OreDeplasare = r.OreDeplasare,
          OreStudiu = r.OreStudiu,
          TaskuriLucrate = r.TaskuriLucrate.Select(t => t.WorkTask.FileNumber).ToList(),
        });

      return dailyEventDtos.Concat(raportDtos).ToList();
    }


  }
}

