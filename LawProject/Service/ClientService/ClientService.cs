using LawProject.Database;
using LawProject.DTO;
using LawProject.Models;
using LawProject.Service.DailyEventService;
using LawProject.Service.FileService;
using LawProject.Service.RaportService;
using LawProject.Service.TaskService;
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

    public ClientService(ApplicationDbContext context, IFileManagementService fileManagementService, ITaskService taskService, IDailyEventService dailyEventService, IRaportService raportService)
    {
      _context = context;
      _fileManagementService = fileManagementService;
      _taskService = taskService;
      _dailyEventService = dailyEventService;
      _raportService = raportService;
    }

    public async Task<IEnumerable<DailyEventDto>> GetAllPFAsync()
    {
      var clients = await _context.ClientPFs.ToListAsync();  
      return clients.Select(client => new DailyEventDto
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
    public async Task AddClientPF(DailyEventDto clientDto)
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

    public async Task<FisaClientDetaliataDto> GetFisaClientDetaliataAsync(int clientId, string clientType, string clientName)
    {
      var files = await _fileManagementService.GetFilesForClientAsync(clientId); 
      var dailyEvents = await _dailyEventService.GetEventsByClient(clientName); 
      var closedTasks = await _taskService.GetClosedTasksByClient(clientId, clientType);
      var rapoarte = await _raportService.GetRapoarteByClientAsync(clientId, clientType);

      return new FisaClientDetaliataDto
      {
        ClientId = clientId,
        ClientType = clientType,
        ClientName = clientName,
        Files = files,
        DailyEvents = dailyEvents,
        ClosedTasks = closedTasks,
        Rapoarte = rapoarte
      };
    }


  }
}
