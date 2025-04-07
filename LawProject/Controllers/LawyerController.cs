using LawProject.Database;
using LawProject.DTO;
using LawProject.Models;
using LawProject.Service.Lawyer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LawProject.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class LawyerController : ControllerBase
  {
    private readonly ILawyerService _lawyerService;

    public LawyerController(ILawyerService lawyerService)
    {
      _lawyerService = lawyerService;
    }

    // GET: api/lawyer
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LawyerDto>>> GetLawyers()
    {
      var lawyers = await _lawyerService.GetAllLawyersAsync();
      return Ok(lawyers);
    }

    // POST: api/lawyer
    [HttpPost]
    public async Task<ActionResult<LawyerDto>> AddLawyer([FromBody] LawyerDto lawyerDto)
    {
      try
      {
        var addedLawyer = await _lawyerService.AddLawyerAsync(lawyerDto);
        return CreatedAtAction(nameof(GetLawyers), new { id = addedLawyer.Id }, addedLawyer);
      }
      catch (ArgumentException ex)
      {
        return BadRequest(ex.Message); // Returnăm mesajul de eroare
      }
    }
  }
}
