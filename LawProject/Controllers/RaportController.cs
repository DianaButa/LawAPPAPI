using LawProject.Database;
using LawProject.DTO;
using LawProject.Models;
using LawProject.Service.DailyEventService;
using LawProject.Service.FileService;
using LawProject.Service.RaportService;
using LawProject.Service.TaskService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LawProject.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class RaportController : ControllerBase
  {
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TaskController> _logger;
    private readonly IRaportService _raportService;
    private readonly IFileManagementService _fileService;
    private readonly ITaskService _taskService;
    private readonly IDailyEventService _dailyEventService;
  
    public RaportController(ApplicationDbContext context, IRaportService raportService, ILogger<TaskController> logger, IFileManagementService fileService, ITaskService taskService, IDailyEventService dailyEventService)
    {
      _context = context;
      _raportService = raportService;
      _logger = logger;
      _fileService = fileService;
      _taskService = taskService;
      _dailyEventService= dailyEventService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateRaport( RaportCreateDto dto)
    {
      var id = await _raportService.CreateRaportAsync(dto);
      return Ok(new { Id = id });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
      var rapoarte = await _raportService.GetAllRapoarteAsync();
      return Ok(rapoarte);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
      var raport = await _raportService.GetRaportByIdAsync(id);
      if (raport == null)
        return NotFound();

      return Ok(raport);
    }

    [HttpGet("fileNumber")]
    public async Task<IActionResult> GetRaportByFileNumber(string fileNumber)
    {
      var raport = await _raportService.GetRaportByFileNumberAsync(fileNumber);
      if (raport == null)
        return NotFound();

      return Ok(raport);
    }

    [HttpGet("byClientId")]
    public async Task<IActionResult> GetRapoarteByClient([FromQuery] int clientId, [FromQuery] string clientType)
    {
      try
      {
        var rapoarte = await _raportService.GetRapoarteByClientAsync(clientId, clientType);
        return Ok(rapoarte);
      }
      catch (Exception ex)
      {
        _logger.LogError($"Eroare la obținerea rapoartelor pentru client: {ex.Message}");
        return StatusCode(500, "Eroare internă la preluarea rapoartelor.");
      }
    }


    [HttpGet("getRapoarteGenerale")]
    public async Task<IActionResult> GetRapoarteGenerale()
    {
      var result = await _raportService.GetRapoarteGeneraleAsync();
      return Ok(result);
    }


    [HttpGet("getRapoarteGenerale-by-lawyer")]
    public async Task<IActionResult> GetRapoarteGeneraleByLawyer([FromQuery] int lawyerId)
    {
      var result = await _raportService.GetRapoarteGeneraleByLawyerAsync(lawyerId);
      return Ok(result);
    }






  }
}
