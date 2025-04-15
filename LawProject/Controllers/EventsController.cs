using LawProject.DTO;
using LawProject.Service.EventService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LawProject.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class EventsController : ControllerBase
  {
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService)
    {
      _eventService = eventService;
    }

    // Adaugă un eveniment Audiere
    [HttpPost("Audiere")]
    public async Task<IActionResult> AddEventA([FromBody] EventADTO eventADto)
    {
      if (eventADto == null)
      {
        return BadRequest("Invalid event data.");
      }

      if (eventADto.Date <= DateTime.MinValue)
      {
        return BadRequest("Invalid date.");
      }

      // Asigură-te că Time este valid, dar nu îl transformi încă în string
      if (eventADto.Time == null || eventADto.Time == TimeSpan.Zero)
      {
        return BadRequest("Invalid time format. Use HH:mm.");
      }

      // Trimitem TimeSpan în loc de string
      var eventToSave = new EventADTO
      {
        Id = eventADto.Id,
        Date = eventADto.Date,
        Time = eventADto.Time,  // Pasăm TimeSpan direct
        Description = eventADto.Description
      };

      var addedEventA = await _eventService.AddEventAAsync(eventToSave);
      return Ok(addedEventA);
    }


    // Adaugă un eveniment Consultanta
    [HttpPost("Consultanta")]
    public async Task<IActionResult> AddEventC([FromBody] EventCDTO eventCDto)
    {
      if (eventCDto == null)
      {
        return BadRequest("Invalid event data.");
      }

      if (eventCDto.Date <= DateTime.MinValue)
      {
        return BadRequest("Invalid date.");
      }

      if (!TimeSpan.TryParse(eventCDto.Time.ToString(), out TimeSpan parsedTime)) 
      {
        return BadRequest("Invalid time format. Use HH:mm.");
      }

      // Trimitem mai departe `parsedTime` în loc de string
      var eventToSave = new EventCDTO
      {
        Id = eventCDto.Id,
        Date = eventCDto.Date,
        Time = parsedTime,
        Description = eventCDto.Description
      };

      var addedEventC = await _eventService.AddEventCAsync(eventToSave);
      return Ok(addedEventC);
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



  

