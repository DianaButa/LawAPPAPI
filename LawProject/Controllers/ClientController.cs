using LawProject.Database;
using LawProject.DTO;
using LawProject.Service.ClientService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LawProject.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class ClientController : ControllerBase
  {
    public readonly ApplicationDbContext _context;
    public readonly IClientService _clientService;

    public ClientController(ApplicationDbContext context, IClientService clientService)
    {
      _context = context;
      _clientService = clientService;
    }

    // Obține persoanele fizice
    [HttpGet("persoane-fizice")]
    public async Task<ActionResult<IEnumerable<ClientPFDto>>> GetClientPF()
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
    public async Task<IActionResult> AddClientPF(ClientPFDto clientDto)
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
  }
}
