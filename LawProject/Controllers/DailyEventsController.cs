using LawProject.Database;
using LawProject.DTO;
using LawProject.Service.DailyEventService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LawProject.Controllers
{
  [Authorize(Roles = "Manager,User,Secretariat")]

  [Route("api/[controller]")]
  [ApiController]
  public class DailyEventsController : ControllerBase
  {
    private readonly ApplicationDbContext _context;
    private readonly IDailyEventService _dailyEventService;
    private readonly ILogger<TaskController> _logger;


    public DailyEventsController(ApplicationDbContext context, IDailyEventService dailyEventService, ILogger<TaskController> logger)
    {
      _context = context;
      _dailyEventService = dailyEventService;
      _logger = logger;
    }


    [HttpPost]
    [Authorize]
    public async Task<ActionResult> AddDailyEvent(DailyEventsDto dto)
    {
      if (dto == null)
      {
        return BadRequest("Datele evenimentului sunt invalide.");
      }

      var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
      if (userIdClaim == null)
      {
        return Unauthorized("UserId missing in token.");
      }

      int userId = int.Parse(userIdClaim.Value);

      // Caută avocatul asociat userului
      var lawyer = await _context.Lawyers.FirstOrDefaultAsync(l => l.UserId == userId);
      if (lawyer == null)
      {
        return BadRequest("Avocatul asociat user-ului nu a fost găsit.");
      }

      // Setează lawyerId în DTO
      dto.LawyerId = lawyer.Id;

      await _dailyEventService.AddDailyEventAsync(dto);
      return Ok("Eveniment adăugat cu succes.");
    }




    [HttpGet("by-fileNumber")]
    public async Task<ActionResult<List<DailyEventsDto>>> GetDailyEventAbyFileNumber(string fileNumber)
    {
      try
      {
        var events = await _dailyEventService.GetEventsByFileNumber(fileNumber);
        return Ok(events);
      }
      catch (Exception ex)
      {
        _logger.LogError($"Eroare la obținerea task-urilor pentru clientul cu ID {fileNumber}: {ex.Message}");
        return BadRequest(ex.Message);
      }
    }

    [HttpGet("DailybyClientId")]
    public async Task<ActionResult<IEnumerable<EventADTO>>> GetDailyEventCbyClientId(string clientName)
    {
      try
      {
        var events = await _dailyEventService.GetEventsByClient(clientName);
        return Ok(events);
      }
      catch (Exception ex)
      {
        _logger.LogError($"Eroare la obținerea task-urilor pentru clientul cu ID {clientName}: {ex.Message}");
        return BadRequest(ex.Message);
      }
    }

    // GET: api/DailyEvents/{lawyerId}
    [HttpGet("lawyer")]
    public async Task<ActionResult<List<DailyEventsDto>>> GetDailyEventsByLawyerId(int lawyerId)
    {
      var events = await _dailyEventService.GetDailyEventsByLawyerIdAsync(lawyerId);
      if (events == null || events.Count == 0)
      {
        return NotFound($"Nu s-au găsit evenimente pentru avocatul cu ID-ul {lawyerId}.");
      }
      return Ok(events);
    }
  }
}
