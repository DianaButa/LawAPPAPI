using LawProject.DTO;
using LawProject.Service.InvoiceSerices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LawProject.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  [Authorize(Roles = "Secretariat,Manager")]
  public class InvoiceController : ControllerBase
  {
    private readonly IInvoiceService _invoiceService;

    public InvoiceController(IInvoiceService invoiceService)
    {
      _invoiceService = invoiceService;
    }


    [HttpPost]
    public async Task<IActionResult> GenereazaFactura([FromBody] InvoiceDto dto)
    {
      var factura = await _invoiceService.GenereazaFacturaAsync(dto);
      return Ok(factura);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllInvoices()
    {
      try
      {
       
        var invoices = await _invoiceService.GetAllInvoicesAsync();

        
        return Ok(invoices);
      }
      catch (Exception ex)
      {
        return BadRequest(new
        {
          Message = ex.Message,
          ExceptionType = ex.GetType().FullName,
          StackTrace = ex.StackTrace
        });
      }

    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetInvoiceById(int id)
    {
      try
      {
        var invoice = await _invoiceService.GetInvoiceByIdAsync(id);

        if (invoice == null)
        {
          return NotFound(new { Message = "Factura nu a fost găsită." });
        }

      
        return Ok(invoice);
      }
      catch (Exception ex)
      {
  
        return BadRequest(new
        {
          Message = ex.Message,
          ExceptionType = ex.GetType().FullName,
          StackTrace = ex.StackTrace
        });
      }
    }

    [HttpGet("scadenta")]
    public async Task<IActionResult> GetInvoicesbyDataScadenta()
    {
      try
      {
        var invoices = await _invoiceService.GetInvoicesByDataScadentaAsync();

        if (invoices == null || !invoices.Any())
        {
          return NotFound(new { Message = "Nu există facturi cu data scadentă de azi încolo." });
        }

        return Ok(invoices);
      }
      catch (Exception ex)
      {
        return BadRequest(new
        {
          Message = ex.Message,
          ExceptionType = ex.GetType().FullName,
          StackTrace = ex.StackTrace
        });
      }
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelInvoice(int id)
    {
      await _invoiceService.CancelInvoiceAsync(id);
      return Ok();
    }

    [HttpPost("{id}/stornare")]
    public async Task<IActionResult> StornareFactura(int id)
    {
      var stornoInvoice = await _invoiceService.StornareFacturaAsync(id);
      return Ok(stornoInvoice);
    }


  }
}
