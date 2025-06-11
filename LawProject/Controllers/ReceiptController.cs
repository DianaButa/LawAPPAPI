using LawProject.DTO;
using LawProject.Service.ReceiptService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LawProject.Controllers
{

  [Authorize(Roles = "Manager,Secretariat")]

  [Route("api/[controller]")]
  [ApiController]
  public class ReceiptController : ControllerBase
  {
    private readonly IReceiptService _receiptService;

    public ReceiptController(IReceiptService receiptService)
    {
      _receiptService = receiptService;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateReceiptAsync([FromBody] ReceiptCreateDto receiptDto)
    {
      if (receiptDto == null)
        return BadRequest("Datele pentru chitanță sunt invalide.");

      try
      {
        var receipt = await _receiptService.GenereazaChitantaAsync(receiptDto);

        return CreatedAtAction(nameof(GetReceiptById), new { numarChitanta = receipt.NumarChitanta }, receipt);
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

    [HttpGet("{numarChitanta}")]
    public async Task<IActionResult> GetReceiptById(string numarChitanta)
    {
      var receipt = await _receiptService.GetReceiptByNumarChitantaAsync(numarChitanta);

      if (receipt == null)
        return NotFound();

      return Ok(receipt);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllReceipts()
    {
      var receipts = await _receiptService.GetAllReceiptsAsync();
      return Ok(receipts);
    }
  }
}

    
  

