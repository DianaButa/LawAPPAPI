using LawProject.Database;
using LawProject.DTO;
using LawProject.Models;
using LawProject.Service.RaportService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LawProject.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class RaportController : ControllerBase
  {
    private readonly ApplicationDbContext _context;
    private readonly IRaportService _raportService;
    public RaportController(ApplicationDbContext context, IRaportService raportService)
    {
      _context = context;
      _raportService = raportService;
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

    [HttpGet("getRapoarteGenerale")]
    public async Task<IActionResult> GetRapoarteGenerale()
    {
      var result = await _raportService.GetRapoarteGeneraleAsync();
      return Ok(result);
    }

  }
}
