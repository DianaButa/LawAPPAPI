using DocumentFormat.OpenXml.Wordprocessing;
using LawProject.Database;
using LawProject.DTO;
using LawProject.Service.ClientService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace LawProject.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class ClientController : ControllerBase
  {
    public readonly ApplicationDbContext _context;
    private readonly ILogger<TaskController> _logger;
    public readonly IClientService _clientService;

    public ClientController(ApplicationDbContext context, IClientService clientService, ILogger<TaskController> logger)
    {
      _context = context;
      _clientService = clientService;
      _logger = logger;
    }

    // Obține persoanele fizice
    [HttpGet("persoane-fizice")]
    public async Task<ActionResult<IEnumerable<DailyEventDto>>> GetClientPF()
    {
      var clients = await _clientService.GetAllPFAsync();
      if (clients == null || !clients.Any())
      {
        return NotFound();
      }
      return Ok(clients);
    }

    // Obține persoanele juridice
    [HttpGet("persoane-juridice")]
    public async Task<ActionResult<IEnumerable<ClientPJDto>>> GetClientPJ()
    {
      var clients = await _clientService.GetAllPJAsync();
      if (clients == null || !clients.Any())
      {
        return NotFound();
      }
      return Ok(clients);
    }

    // Adăugare client persoană fizică
    [HttpPost("persoane-fizice")]
    public async Task<IActionResult> AddClientPF(DailyEventDto clientDto)
    {
      try
      {
        await _clientService.AddClientPF(clientDto);
        return Ok("Client persoană fizică adăugat cu succes.");
      }
      catch (Exception ex)
      {
        return StatusCode(StatusCodes.Status500InternalServerError, $"Eroare: {ex.Message}");
      }
    }

    // Adăugare client persoană juridică
    [HttpPost("persoane-juridice")]
    public async Task<IActionResult> AddClientPJ(ClientPJDto clientDto)
    {
      try
      {
        await _clientService.AddClientPJ(clientDto);
        return Ok("Client persoană juridică adăugat cu succes.");
      }
      catch (Exception ex)
      {
        return StatusCode(StatusCodes.Status500InternalServerError, $"Eroare: {ex.Message}");
      }
    }

    [HttpGet("fisa-client")]
    public async Task<IActionResult> GetFisaClientDetaliata(
    [FromQuery] int clientId,
    [FromQuery] string clientType,
    [FromQuery] string clientName,
    [FromQuery]  DateTime? startDate,
    [FromQuery] DateTime? endDate)
    {
      try
      {
        var fisa = await _clientService.GetFisaClientDetaliataAsync(clientId, clientType, clientName, startDate, endDate);
        return Ok(fisa);
      }
      catch (Exception ex)
      {
        _logger.LogError($"Eroare la generarea fișei clientului: {ex.Message}");
        return StatusCode(500, "Eroare la generarea fișei clientului.");
      }
    }

    [HttpGet("activitate-dosar")]
    public async Task<IActionResult> GetAllDataByFileNumber([FromQuery] string fileNumber, [FromQuery] DateTime? startDate,
    [FromQuery] DateTime? endDate)
    {
      var decodedFileNumber = Uri.UnescapeDataString(fileNumber);
      var data = await _clientService.GetFullDataByFileNumberAsync(decodedFileNumber, startDate, endDate);
      var fileStatus = data.Files?.FirstOrDefault(f => f.FileNumber == decodedFileNumber)?.Status;

      if (data == null)
        return NotFound($"No data found for file number: {decodedFileNumber}");

      return Ok(new
      {
        data.FileNumber,
        data.FilesCount,
        data.FileStatus,
        data.DailyEventsCount,
        data.ClosedTasksCount,
        data.OpenedTasksCount,
        data.RapoarteCount,
        data.Files,
        data.DailyEvents,
        data.ClosedTasks,
        data.OpenedTasks,
        data.Rapoarte
      });
    }


    [HttpGet("dateptedit")]
    public async Task<IActionResult> GetClient([FromQuery] int clientId, [FromQuery] string clientType)
    {
      try
      {
        _logger.LogInformation($"Primim GET pentru clientId={clientId}, clientType={clientType}");

        clientType = clientType?.Trim();  // NU facem ToLower aici

        if (string.IsNullOrEmpty(clientType) || (clientType != "PF" && clientType != "PJ"))
          return BadRequest("Tipul clientului este invalid. Se acceptă doar 'PF' sau 'PJ'.");

        var client = await _clientService.GetClientEntityByIdAndTypeAsync(clientId, clientType);
        if (client == null)
          return NotFound("Clientul nu a fost găsit.");

        return Ok(client);
      }
      catch (ArgumentException ex)
      {
        _logger.LogError($"Eroare la GET client: {ex.Message}");
        return BadRequest(ex.Message);
      }
      catch (Exception ex)
      {
        _logger.LogError($"Eroare neașteptată la GET client: {ex.Message}");
        return StatusCode(500, "Eroare la obținerea datelor clientului.");
      }
    }


    // PUT: api/clients?clientId=1&clientType=pf
    [HttpPut]
    public async Task<IActionResult> UpdateClient([FromQuery] int clientId, [FromQuery] string clientType, [FromBody] JsonElement clientDto)
    {
      try
      {
        _logger.LogInformation($"Primim PUT pentru clientId={clientId}, clientType={clientType}");

        clientType = clientType?.Trim().ToLower();
        if (string.IsNullOrEmpty(clientType) || (clientType != "pf" && clientType != "pj"))
          return BadRequest("Tipul clientului este invalid. Se acceptă doar 'pf' sau 'pj'.");

        if (clientType == "pf")
        {
          var pfDto = clientDto.Deserialize<DailyEventDto>();
          await _clientService.UpdateClientPF(clientId, pfDto);
        }
        else
        {
          var pjDto = clientDto.Deserialize<ClientPJDto>();
          await _clientService.UpdateClientPJ(clientId, pjDto);
        }

        return Ok("Client actualizat cu succes.");
      }
      catch (Exception ex)
      {
        _logger.LogError($"Eroare la actualizarea clientului: {ex.Message}");
        return StatusCode(500, "Eroare la actualizarea clientului.");
      }
    }
  



  // Ștergere client (PF sau PJ)
  [HttpDelete]
    public async Task<IActionResult> DeleteClient([FromQuery] int clientId, [FromQuery] string clientType)
    {
      try
      {
        if (clientType.ToLower() == "pf")
        {
          await _clientService.DeleteClientPF(clientId);
        }
        else if (clientType.ToLower() == "pj")
        {
          await _clientService.DeleteClientPJ(clientId);
        }
        else
        {
          return BadRequest("Tipul clientului este invalid. Se acceptă doar 'pf' sau 'pj'.");
        }

        return Ok("Client șters cu succes.");
      }
      catch (Exception ex)
      {
        _logger.LogError($"Eroare la ștergerea clientului: {ex.Message}");
        return StatusCode(500, "Eroare la ștergerea clientului.");
      }
    }


  }
}
