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

    public ClientController(ApplicationDbContext context , IClientService clientService)
    {
      _context = context;
      _clientService = clientService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClientDto>>> GetClient()
    {
      var files = await _clientService.GetAllAsync();
      if (files == null || !files.Any())
      {
        return NotFound();
      }
      return Ok(files);
    }
    [HttpPost("add")]
    public async Task<IActionResult> AddClient(ClientDto clientDto)
    {
      try
      {
        await _clientService.AddClient(clientDto);
        return Ok("Product added successfully.");
      }
      catch (System.Exception ex)
      {
        return StatusCode(StatusCodes.Status500InternalServerError, $"Error adding product: {ex.Message}");
      }
    }




  }
}
