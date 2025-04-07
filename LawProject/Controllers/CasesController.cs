using LawProject.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceReference1;


namespace LawProject.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class CasesController : ControllerBase
  {
    private readonly MyQueryService _queryService;

    public CasesController(MyQueryService queryService)
    {
      _queryService = queryService;
    }

    [HttpGet("dosare")]
    public async Task<IActionResult> GetDosare([FromQuery] string numarDosar, [FromQuery] string? obiectDosar = null, [FromQuery] string? numeParte = null, [FromQuery] Institutie? institutie = null, [FromQuery] DateTime? dataStart = null, [FromQuery] DateTime? dataStop = null)
    {
      if (string.IsNullOrEmpty(numarDosar))
      {
        return BadRequest("Numărul dosarului este obligatoriu.");
      }

      try
      {
        var data = await _queryService.CautareDosareAsync(numarDosar, obiectDosar, numeParte, institutie, dataStart, dataStop);
        if (data == null || data.Length == 0)
        {
          return NotFound("No data found for the given criteria.");
        }
        return Ok(data);
      }
      catch (Exception ex)
      {
        // Log the exception
        return StatusCode(500, $"Internal server error: {ex.Message}");
      }
    }

    [HttpGet("sedinte")]
    public async Task<IActionResult> GetSedinte([FromQuery] DateTime dataSedinta, [FromQuery] Institutie institutie)
    {
      try
      {
        var data = await _queryService.CautareSedinteAsync(dataSedinta, institutie);
        if (data == null || data.Length == 0)
        {
          return NotFound("No data found for the given criteria.");
        }
        return Ok(data);
      }
      catch (Exception ex)
      {
        // Log the exception
        return StatusCode(500, $"Internal server error: {ex.Message}");
      }
    }
  }
}
