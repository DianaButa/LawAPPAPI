using LawProject.DTO;
using LawProject.Service.POSService;
using LawProject.Service.ReceiptService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LawProject.Controllers
{
  [Authorize(Roles = "Manager,Secretariat")]

  [Route("api/[controller]")]
  [ApiController]

  public class POSController : ControllerBase
  {
    private readonly IPOSService _posService;

  public POSController(IPOSService posService)
  {
    _posService = posService;
  }

  [HttpPost("generate")]
  public async Task<IActionResult> GeneratePOSAsync([FromBody] POSCreateDto posDto)
  {
    if (posDto == null)
      return BadRequest("Datele pentru chitanță sunt invalide.");

    try
    {
      var incasare = await _posService.GenereazaIncasareAsync(posDto);

      return CreatedAtAction(nameof(GetPOSById), new { numarIncasare = incasare.NumarIncasare }, incasare);
    }
    catch (ArgumentException ex)
    {
      return BadRequest(ex.Message);
    }
    catch (Exception ex)
    {
      return StatusCode(500, "A apărut o eroare neașteptată: " + ex.Message);
    }
  }

  [HttpGet("{numarIncasare}")]
  public async Task<IActionResult> GetPOSById(string numarIncasare)
  {
    var incasari = await _posService.GetPOSByNumarIncasareAsync(numarIncasare);

    if (incasari == null)
      return NotFound();

    return Ok(incasari);
  }

  [HttpGet]
  public async Task<IActionResult> GetAllPOS()
  {
    var incasari = await _posService.GetAllPOSsAsync();
    return Ok(incasari);
  }
}
}

    
  


