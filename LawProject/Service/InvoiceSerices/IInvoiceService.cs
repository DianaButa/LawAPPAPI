using LawProject.DTO;
using LawProject.Models;

namespace LawProject.Service.InvoiceSerices
{
  public interface IInvoiceService
  {

    Task<Invoice> GenereazaFacturaAsync(InvoiceDto dto);

    Task<List<InvoiceDto>> GetAllInvoicesAsync();

    Task<Invoice> GetInvoiceByIdAsync(int id);

    Task<IEnumerable<InvoiceDto>> GetInvoicesByDataScadentaAsync();
    Task CancelInvoiceAsync(int invoiceId);
    Task<Invoice> StornareFacturaAsync(int invoiceId);
  }
  }

