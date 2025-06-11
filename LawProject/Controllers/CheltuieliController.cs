using LawProject.DTO;
using LawProject.Service.CheltuieliService;
using LawProject.Service.POSService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LawProject.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  [Authorize(Roles = "Secretariat,Manager")]
  public class CheltuieliController : ControllerBase
  {

      private readonly ICheltuieliService _cheltuieliService;

      public CheltuieliController(ICheltuieliService cheltuieliService)
      {
        _cheltuieliService = cheltuieliService;
      }

      [HttpPost]
      public async Task<IActionResult> AddCheltuieliAsync([FromBody] CheltuieliDto cheltuieliDto)
      {
      if (cheltuieliDto == null)
          return BadRequest("Datele pentru cheltuieli sunt invalide.");

      try
      {
        await _cheltuieliService.AddCheltuieliAsync(cheltuieliDto);
        return Ok("Cheltuiala adăugata cu succes.");
      }
      catch (Exception ex)
      {
        return StatusCode(StatusCodes.Status500InternalServerError, $"Eroare: {ex.Message}");
      }
    }

  

      [HttpGet]
      public async Task<IActionResult> GetAllCheltuieli()
      {
        var incasari = await _cheltuieliService.GetAllCheltuieliAsync();
        return Ok(incasari);
      }
    }
  }


