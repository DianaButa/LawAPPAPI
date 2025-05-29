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
      // Verifică dacă avocatul există
      var lawyer = await _context.Lawyers.FirstOrDefaultAsync(l => l.Id == dto.LawyerId);
      if (lawyer == null)
        throw new ArgumentException($"Avocatul cu ID {dto.LawyerId} nu a fost găsit.");

      // Construiește raportul
      var raport = new Raport
      {
        LawyerId = dto.LawyerId,
        LawyerName = lawyer.LawyerName,
        DataRaport = dto.DataRaport,
        OreDeplasare = dto.OreDeplasare,
        OreInstanta = dto.OreInstanta,
        OreAudieri = dto.OreAudieri,
        OreConsultante = dto.OreConsultante,
        OreAlteActivitati = dto.OreAlteActivitati,

        // Taskuri multiple
        TaskuriLucrate = dto.Taskuri?.Select(t => new RaportTask
        {
          WorkTaskId = t.WorkTaskId,
          WorkTaskTitle=t.WorkTaskTitle,
          WorkTaskFileNumber = t.WorkTaskFileNumber,
          OreLucrate = t.OreLucrate ?? 0
        }).ToList(),

        // Studii pe dosare multiple
        StudiiPeDosar = dto.StudiiPeDosar?.Select(s => new RaportStudiuDosar
        {
          FileId = s.FileId ?? 0,
          FileNumber = s.FileNumber,
          OreStudiu = s.OreStudiu ?? 0
        }).ToList()
      };

      _context.Rapoarte.Add(raport);
      await _context.SaveChangesAsync();

      return raport.Id;
    }

    public async Task<List<RaportDto>> GetAllRapoarteAsync()
    {
      var rapoarte = await _context.Rapoarte
        .Include(r => r.TaskuriLucrate)
        .Include(r => r.StudiiPeDosar)
        .Include(r => r.Lawyer)
        .ToListAsync();

      var result = rapoarte.Select(r => new RaportDto
      {
        Id = r.Id,
        LawyerName = r.Lawyer?.LawyerName ?? "",
        LawyerId = r.LawyerId,
        DataRaport = r.DataRaport,
        OreDeplasare = r.OreDeplasare,
        OreInstanta = r.OreInstanta,
        OreAudieri = r.OreAudieri,
        OreConsultante = r.OreConsultante,
        OreAlteActivitati = r.OreAlteActivitati,

        Taskuri = r.TaskuriLucrate?.Select(t => new RaportTaskDto
        {
          WorkTaskId = t.WorkTaskId,
          WorkTaskTitle=t.WorkTaskTitle,
          WorkTaskFileNumber = t.WorkTaskFileNumber,
          OreLucrate = t.OreLucrate
        }).ToList() ?? new List<RaportTaskDto>(),

        StudiiPeDosar = r.StudiiPeDosar?.Select(s => new RaportStudiuDosarDto
        {
          FileId = s.FileId,
          FileNumber = s.FileNumber,
          OreStudiu = s.OreStudiu
        }).ToList() ?? new List<RaportStudiuDosarDto>()
      }).ToList();

      return result;
    }

    public async Task<List<RaportDto>> GetRapoarteByLawyerIdAsync(int lawyerId)
    {
      var rapoarte = await _context.Rapoarte
        .Where(r => r.LawyerId == lawyerId)
        .Include(r => r.TaskuriLucrate)
        .Include(r => r.StudiiPeDosar)
        .Include(r => r.Lawyer)
        .ToListAsync();

      var result = rapoarte.Select(r => new RaportDto
      {
        Id = r.Id,
        LawyerName = r.Lawyer?.LawyerName ?? "",
        LawyerId = r.LawyerId,
        DataRaport = r.DataRaport,
        OreDeplasare = r.OreDeplasare,
        OreInstanta = r.OreInstanta,
        OreAudieri = r.OreAudieri,
        OreConsultante = r.OreConsultante,
        OreAlteActivitati = r.OreAlteActivitati,

        Taskuri = r.TaskuriLucrate?.Select(t => new RaportTaskDto
        {
          WorkTaskId = t.WorkTaskId,
          WorkTaskTitle=t.WorkTaskTitle,
          WorkTaskFileNumber=t.WorkTaskFileNumber,
          OreLucrate = t.OreLucrate
        }).ToList() ?? new List<RaportTaskDto>(),

        StudiiPeDosar = r.StudiiPeDosar?.Select(s => new RaportStudiuDosarDto
        {
          FileId = s.FileId,
          FileNumber = s.FileNumber,
          OreStudiu = s.OreStudiu
        }).ToList() ?? new List<RaportStudiuDosarDto>()
      }).ToList();

      return result;
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
        ClientType = e.ClientType,
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
        LawyerId = r.LawyerId,
        LawyerName = r.LawyerName,
        Date = r.DataRaport != DateTime.MinValue ? r.DataRaport : r.Date, 

        OreDeplasare = r.OreDeplasare,
        OreStudiu = r.OreStudiu,
        OreInstanta = r.OreInstanta,
        OreAudieri = r.OreAudieri,
        OreConsultante = r.OreConsultante,
        OreAlteActivitati = r.OreAlteActivitati,

        TaskuriLucrate = r.Taskuri ?? new List<RaportTaskDto>(),

        StudiiPeDosar = r.StudiiPeDosar ?? new List<RaportStudiuDosarDto>(),

        // Optional poți adăuga alte câmpuri dacă ai în DTO-ul general
      });

      return dailyEventDtos.Concat(raportDtos).ToList();
    }


    public async Task<List<RaportGeneralDto>> GetRapoarteGeneraleByLawyerAsync(int lawyerId)
    {
      var dailyEvents = await _dailyEventService.GetAllDailyEventsAsync();
      var rapoarte = await GetAllRapoarteAsync();

      // DAILY EVENTS filtrate
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

      // RAPOARTE filtrate
      var raportDtos = rapoarte
   .Where(r => r.LawyerId == lawyerId)
   .Select(r => new RaportGeneralDto
   {
     TipRaport = "Raport",
     RaportId = r.Id,
     LawyerId = r.LawyerId,
     LawyerName = r.LawyerName,
     Date = r.DataRaport,

     OreDeplasare = r.OreDeplasare,
     OreStudiu = r.OreStudiu,
     OreInstanta = r.OreInstanta,
     OreAudieri = r.OreAudieri,
     OreConsultante = r.OreConsultante,
     OreAlteActivitati = r.OreAlteActivitati,

     // Extragere ID-uri taskuri
     TaskuriLucrate = r.Taskuri ?? new List<RaportTaskDto>(),

     StudiiPeDosar = r.StudiiPeDosar?.Select(s => new RaportStudiuDosarDto
     {
       FileId = s.FileId,
       FileNumber = s.FileNumber,
       OreStudiu = s.OreStudiu
     }).ToList() ?? new List<RaportStudiuDosarDto>()
   });

      return dailyEventDtos.Concat(raportDtos).ToList();
    }

  }
}
