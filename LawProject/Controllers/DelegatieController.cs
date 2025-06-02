using LawProject.DTO;
using LawProject.Service.DelegatieService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LawProject.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class DelegatieController : ControllerBase
  {
    private readonly IDelegatieService _delegatieService;

    public DelegatieController(IDelegatieService delegatieService)
    {
      _delegatieService = delegatieService;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateDelegatie([FromBody] DelegatieDto dto)
    {
     
        if (!ModelState.IsValid)
        {
          return BadRequest(ModelState);
        }

        var fileBytes = await _delegatieService.GenerateDocumentAsync(dto);

        if (fileBytes == null)
        {
          return BadRequest("A apărut o eroare la generarea documentului.");
        }

        string fileName = $"Contract_{dto.ClientId}.docx";
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName);

      }
      }
  }

