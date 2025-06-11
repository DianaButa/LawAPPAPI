using LawProject.DTO;
using LawProject.Models;
using LawProject.Service.EventService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LawProject.Controllers
{
  [Authorize(Roles = "Manager,User,Secretariat")]

  [Route("api/[controller]")]
  [ApiController]
  public class EventsController : ControllerBase
  {
    private readonly IEventService _eventService;
    private readonly ILogger<TaskController> _logger;

    public EventsController(IEventService eventService, ILogger<TaskController> logger)
    {
      _eventService = eventService;
      _logger = logger;
    }

    // Adaugă un eveniment Audiere
    [HttpPost("Audiere")]
    public async Task<IActionResult> AddEventA([FromBody] EventADTO eventADto)
    {
      if (eventADto == null)
        return BadRequest("Invalid event data.");

      try
      {
        var addedEvent = await _eventService.AddEventAAsync(eventADto);
        return Ok(addedEvent);
      }
      catch (ArgumentException ex)
      {
        return BadRequest(ex.Message);
      }
    }

    // POST: api/Events/Consultanta
    [HttpPost("Consultanta")]
    public async Task<IActionResult> AddEventC([FromBody] EventCDTO eventCDto)
    {
      if (eventCDto == null)
        return BadRequest("Invalid event data.");

      try
      {
        var addedEvent = await _eventService.AddEventCAsync(eventCDto);
        return Ok(addedEvent);
      }
      catch (ArgumentException ex)
      {
        return BadRequest(ex.Message);
      }
    }




    // Obține evenimentele A(audiere)
    [HttpGet("Audiere")]
    public async Task<ActionResult<IEnumerable<EventADTO>>> GetEventA()
    {
      var eventsA = await _eventService.GetAllEventsAAsync();
      if (eventsA == null || !eventsA.Any())
      {
        return NotFound();
      }
      return Ok(eventsA);
    }


    [HttpGet("AudierebyClientId")]
    public async Task<ActionResult<IEnumerable<EventADTO>>> GetEventAbyClientId(int clientId, string clientType)
    {
      try
      {
        var eventsA = await _eventService.GetEventAByClient(clientId, clientType);
        return Ok(eventsA);
      }
      catch (Exception ex)
      {
        _logger.LogError($"Eroare la obținerea task-urilor pentru clientul cu ID {clientId}: {ex.Message}");
        return BadRequest(ex.Message);
      }
    }
    [HttpGet("ConsultantabyClientId")]
    public async Task<ActionResult<IEnumerable<EventADTO>>> GetEventCbyClientId(int clientId, string clientType)
    {
      try
      {
        var eventsA = await _eventService.GetEventCByClient(clientId, clientType);
        return Ok(eventsA);
      }
      catch (Exception ex)
      {
        _logger.LogError($"Eroare la obținerea task-urilor pentru clientul cu ID {clientId}: {ex.Message}");
        return BadRequest(ex.Message);
      }
    }



    // Obține evenimentele C(consultanta)
    [HttpGet("Consultanta")]
    public async Task<ActionResult<IEnumerable<EventCDTO>>> GetEventC()
    {
      var eventsC = await _eventService.GetAllEventsCAsync();
      if (eventsC == null || !eventsC.Any())
      {
        return NotFound();
      }
      return Ok(eventsC);
    }
  }
}



  

