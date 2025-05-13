using LawProject.DTO;
using LawProject.Service.ContractService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LawProject.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class ContractController : ControllerBase
  {
    private readonly IContractService _contractService;

    public ContractController(IContractService contractService)
    {
      _contractService = contractService;
    }

    [HttpPost]
    public async Task<IActionResult> GenerateDocument([FromBody] GenerateContractDto dto)
    {
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      var fileBytes = await _contractService.GenerateDocumentsAsync(dto);

      if (fileBytes == null)
      {
        return BadRequest("A apărut o eroare la generarea documentului.");
      }

      string fileName = $"Contract_{dto.ClientId}.docx";
      return File(fileBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName);
    }
  }
}

