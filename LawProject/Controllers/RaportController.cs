using LawProject.Database;
using LawProject.DTO;
using LawProject.Models;
using LawProject.Service.DailyEventService;
using LawProject.Service.FileService;
using LawProject.Service.RaportService;
using LawProject.Service.TaskService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    [Authorize]
    public async Task<IActionResult> CreateRaport(RaportCreateDto dto)
    {
      var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
      if (userIdClaim == null)
      {
        return Unauthorized("UserId missing in token.");
      }
      int userId = int.Parse(userIdClaim.Value);

      var lawyer = await _context.Lawyers.FirstOrDefaultAsync(l => l.UserId == userId);
      if (lawyer == null)
      {
        return BadRequest("Nu există avocat asociat acestui cont.");
      }

      // Setăm LawyerId doar dacă nu este deja trimis
      if (dto.LawyerId == null || dto.LawyerId == 0)
      {
        dto.LawyerId = lawyer.Id;
      }

      var id = await _raportService.CreateRaportAsync(dto);
      return Ok(new { Id = id });
    }


    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
      var rapoarte = await _raportService.GetAllRapoarteAsync();
      return Ok(rapoarte);
    }

    [HttpGet("lawyer")]
    public async Task<ActionResult<List<RaportDto>>> GetRapoarteByLawyerId(int lawyerId)
    {
      var rapoarte = await _raportService.GetRapoarteByLawyerIdAsync(lawyerId);
      if (rapoarte == null || !rapoarte.Any())
      {
        return NotFound($"Nu s-au găsit rapoarte pentru avocatul cu ID-ul {lawyerId}.");
      }
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

    //[HttpGet("byClientId")]
    //public async Task<IActionResult> GetRapoarteByClient([FromQuery] int clientId, [FromQuery] string clientType)
    //{
    //  try
    //  {
    //    var rapoarte = await _raportService.GetRapoarteByClientAsync(clientId, clientType);
    //    return Ok(rapoarte);
    //  }
    //  catch (Exception ex)
    //  {
    //    _logger.LogError($"Eroare la obținerea rapoartelor pentru client: {ex.Message}");
    //    return StatusCode(500, "Eroare internă la preluarea rapoartelor.");
    //  }
    //}


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
