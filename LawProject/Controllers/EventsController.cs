using LawProject.DTO;
using LawProject.Models;
using LawProject.Service.EventService;
using LawProject.Service.FileService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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

    [HttpGet("events/{eventType}/{eventId}")]
    public async Task<IActionResult> GetEventById(string eventType, int eventId)
    {
      if (eventType.ToUpper() == "A")
      {
        var eventA = await _eventService.GetEventAByIdAsync(eventId);
        if (eventA == null) return NotFound();
        return Ok(eventA);
      }
      else if (eventType.ToUpper() == "C")
      {
        var eventC = await _eventService.GetEventCByIdAsync(eventId);
        if (eventC == null) return NotFound();
        return Ok(eventC);
      }
      else
      {
        return BadRequest("Invalid event type");
      }
    }

    [HttpPut("events/{eventType}/{eventId}")]
    public async Task<IActionResult> UpdateEvent(string eventType, int eventId, [FromBody] JsonElement updatedEventDtos)
    {
      if (eventType.ToUpper() == "A")
      {
        var updatedEventA = updatedEventDtos.Deserialize<EventADTO>();
        if (updatedEventA == null) return BadRequest("Invalid data for EventA");

        var updated = await _eventService.UpdateEventAAsync(eventId, updatedEventA);
        if (updated == null) return NotFound();

        return Ok(updated);
      }
      else if (eventType.ToUpper() == "C")
      {
        var updatedEventC = updatedEventDtos.Deserialize<EventCDTO>();
        if (updatedEventC == null) return BadRequest("Invalid data for EventC");

        var updated = await _eventService.UpdateEventCAsync(eventId, updatedEventC);
        if (updated == null) return NotFound();

        return Ok(updated);
      }
      else
      {
        return BadRequest("Invalid event type");
      }
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteEvent([FromQuery] int eventId, [FromQuery] string eventType)
    {
      try
      {
        var cleanedType = eventType?.Trim().ToLower();

        if (cleanedType == "c")
        {
          await _eventService.DeleteEventC(eventId);
        }
        else if (cleanedType == "a")
        {
          await _eventService.DeleteEventA(eventId);
        }
        else
        {
          return BadRequest("Tipul evenimentului este invalid. Se acceptă doar 'C' sau 'A'.");
        }

        return Ok("Eveniment șters cu succes.");
      }
      catch (KeyNotFoundException)
      {
        return NotFound("Evenimentul nu a fost găsit.");
      }
      catch (Exception ex)
      {
        _logger.LogError($"Eroare la ștergerea eventului: {ex.Message}");
        return StatusCode(500, "Eroare la ștergerea eventului.");
      }
    }




  }

}



  

