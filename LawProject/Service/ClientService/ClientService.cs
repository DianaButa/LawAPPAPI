using LawProject.Database;
using LawProject.DTO;
using LawProject.Models;
using LawProject.Service.DailyEventService;
using LawProject.Service.FileService;
using LawProject.Service.RaportService;
using LawProject.Service.TaskService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LawProject.Service.ClientService
{
  public class ClientService : IClientService
  {
    private readonly ApplicationDbContext _context;

    private readonly IFileManagementService _fileManagementService;
    private readonly ITaskService _taskService;
    private readonly IDailyEventService _dailyEventService;
    private readonly IRaportService _raportService;
    private readonly ILogger<ClientService> _logger;


    public ClientService(ApplicationDbContext context, IFileManagementService fileManagementService, ITaskService taskService, IDailyEventService dailyEventService, IRaportService raportService, ILogger<ClientService> logger)
    {
      _context = context;
      _fileManagementService = fileManagementService;
      _taskService = taskService;
      _dailyEventService = dailyEventService;
      _raportService = raportService;
      _logger = logger;
    }

    public async Task<IEnumerable<ClientPFDto>> GetAllPFAsync()
    {
      var clients = await _context.ClientPFs.ToListAsync();  
      return clients.Select(client => new ClientPFDto
      {
        Id = client.Id,
        FirstName = client.FirstName,
        LastName = client.LastName,
        CNP = client.CNP,
        Email = client.Email,
        Address = client.Address,
        PhoneNumber = client.PhoneNumber
      });
    }

    public async Task<IEnumerable<ClientPJDto>> GetAllPJAsync()
    {
      var clients = await _context.ClientPJs.ToListAsync();  
      return clients.Select(client => new ClientPJDto
      {
        Id = client.Id,
        CompanyName = client.CompanyName,
        CUI = client.CUI,
        Email = client.Email,
        Address = client.Address,
        PhoneNumber = client.PhoneNumber
      });
    }

    // Adăugare client persoană fizică
    public async Task AddClientPF(ClientPFDto clientDto)
    {
      var newClient = new ClientPF
      {
        FirstName = clientDto.FirstName,
        LastName = clientDto.LastName,
        CNP = clientDto.CNP,
        Email = clientDto.Email,
        Address = clientDto.Address,
        PhoneNumber = clientDto.PhoneNumber
      };

      _context.ClientPFs.Add(newClient);  
      await _context.SaveChangesAsync();
    }

    
    public async Task AddClientPJ(ClientPJDto clientDto)
    {
      var newClient = new ClientPJ
      {
        CompanyName = clientDto.CompanyName,
        CUI = clientDto.CUI,
        Email = clientDto.Email,
        Address = clientDto.Address,
        PhoneNumber = clientDto.PhoneNumber
      };

      _context.ClientPJs.Add(newClient);  
      await _context.SaveChangesAsync();
    }

    public async Task<FisaClientDetaliataDto> GetFisaClientDetaliataAsync(int clientId, string clientType, string clientName, DateTime? startDate, DateTime? endDate)
    {
      var files = await _fileManagementService.GetFilesForClientAsync(clientId);
      var dailyEvents = await _dailyEventService.GetEventsByClient(clientName);
      var closedTasks = await _taskService.GetClosedTasksByClient(clientId, clientType);

      // 🔎 Aplică filtrarea pe baza intervalului primit
      if (startDate.HasValue && endDate.HasValue)
      {
        dailyEvents = dailyEvents
            .Where(e => e.Date >= startDate.Value && e.Date <= endDate.Value)
            .ToList();

        closedTasks = closedTasks
            .Where(t => t.StartDate >= startDate.Value && t.StartDate <= endDate.Value)
            .ToList();
      }

      return new FisaClientDetaliataDto
      {
        ClientId = clientId,
        ClientType = clientType,
        ClientName = clientName,
        Files = files,
        DailyEvents = dailyEvents,
        ClosedTasks = closedTasks
      };
    }


    public async Task<FullFileDataDto> GetFullDataByFileNumberAsync(string fileNumber, DateTime? startDate, DateTime? endDate)
    {
      var file = await _fileManagementService.GetFileByNumberAsync(fileNumber);
      var fileStatus = file?.Status;

      var events = await _dailyEventService.GetEventsByFileNumber(fileNumber);
      var closedTasks = await _taskService.GetTasksByFileNumberAndClosedStatusAsync(fileNumber);
      var openedTasks = await _taskService.GetTasksByFileNumberAndOpenStatusAsync(fileNumber);
      var raport = await _raportService.GetRaportByFileNumberAsync(fileNumber);

      var rapoarte = raport != null ? new List<Raport> { raport } : new List<Raport>();

      if (startDate.HasValue && endDate.HasValue)
      {
        events = events
            .Where(e => e.Date >= startDate.Value && e.Date <= endDate.Value)
            .ToList();

        closedTasks = closedTasks
            .Where(t => t.StartDate >= startDate.Value && t.StartDate <= endDate.Value)
            .ToList();

        openedTasks = openedTasks
            .Where(t => t.StartDate >= startDate.Value && t.StartDate <= endDate.Value)
            .ToList();

        rapoarte = rapoarte
            .Where(r => r.DataRaport >= startDate.Value && r.DataRaport <= endDate.Value)
            .ToList();
      }

      return new FullFileDataDto
      {
        FileNumber = fileNumber,
        FileStatus = fileStatus,
        DailyEvents = events,
        ClosedTasks = closedTasks,
        OpenedTasks = openedTasks,
        Rapoarte = rapoarte
      };
    }


    public async Task UpdateClientPJ(int clientId, ClientPJDto clientDto)
    {
      var client = await _context.ClientPJs.FindAsync(clientId);
      if (client == null) throw new Exception("Clientul nu a fost găsit.");

      client.CompanyName = clientDto.CompanyName;
      client.CUI = clientDto.CUI;
      client.Email = clientDto.Email;
      client.Address = clientDto.Address;
      client.PhoneNumber = clientDto.PhoneNumber;

      await _context.SaveChangesAsync();
    }

    public async Task UpdateClientPF(int clientId, ClientPFDto clientDto)
    {
      var client = await _context.ClientPFs.FindAsync(clientId);
      if (client == null) throw new Exception("Clientul nu a fost găsit.");

      client.FirstName = clientDto.FirstName;
      client.LastName = clientDto.LastName;
      client.CNP = clientDto.CNP;
      client.Email = clientDto.Email;
      client.Address = clientDto.Address;
      client.PhoneNumber = clientDto.PhoneNumber;

      var changed = await _context.SaveChangesAsync();
      _logger.LogInformation($"Client PF cu id={clientId} actualizat. Entități modificate: {changed}");
    }


    public async Task DeleteClientPF(int clientId)
    {
      var client = await _context.ClientPFs.FindAsync(clientId);
      if (client == null) throw new Exception("Clientul nu a fost găsit.");

      _context.ClientPFs.Remove(client);
      await _context.SaveChangesAsync();
    }

    public async Task DeleteClientPJ(int clientId)
    {
      var client = await _context.ClientPJs.FindAsync(clientId);
      if (client == null) throw new Exception("Clientul nu a fost găsit.");

      _context.ClientPJs.Remove(client);
      await _context.SaveChangesAsync();
    }

    public async Task<object> GetClientEntityByIdAndTypeAsync(int clientId, string clientType)
    {
      clientType = clientType?.Trim();

      if (string.IsNullOrEmpty(clientType) || (clientType != "PF" && clientType != "PJ"))
        throw new ArgumentException("Tip client invalid. Se acceptă doar 'PF' sau 'PJ'.");

      if (clientId <= 0)
        throw new ArgumentException("ID-ul clientului trebuie să fie un număr pozitiv.");

      _logger.LogInformation($"Caut client cu ID={clientId}, Tip={clientType}");

      if (clientType == "PF")
      {
        var clientPF = await _context.ClientPFs.FindAsync(clientId);
        if (clientPF == null)
        {
          _logger.LogWarning($"Client PF cu ID={clientId} nu a fost găsit.");
          return null;
        }
        return clientPF;
      }
      else // clientType == "PJ"
      {
        var clientPJ = await _context.ClientPJs.FindAsync(clientId);
        if (clientPJ == null)
        {
          _logger.LogWarning($"Client PJ cu ID={clientId} nu a fost găsit.");
          return null;
        }
        return clientPJ;
      }
    }



  }

}

