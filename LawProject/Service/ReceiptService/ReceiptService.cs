using LawProject.Database;
using LawProject.DTO;
using LawProject.Models;
using Microsoft.EntityFrameworkCore;

namespace LawProject.Service.ReceiptService
{
  public class ReceiptService : IReceiptService
  {
    private readonly ApplicationDbContext _context;

    public ReceiptService(ApplicationDbContext context)
    {
     _context = context;
    }

    public async Task<ReceiptResponseDto> GenereazaChitantaAsync(ReceiptCreateDto dto)
    {
      var factura = await _context.Invoices
          .FirstOrDefaultAsync(f => f.Id == dto.InvoiceId);

      if (factura == null)
      {
        throw new Exception("Factura nu a fost găsită.");
      }

      int offset = 581; // prima chitanță nouă va fi DAAC-582
      var count = await _context.Receipts.CountAsync();
      var nextNumber = offset + count + 1;

      var numarChitanta = $"DAAC-{nextNumber}";

      var chitanta = new Receipt
      {
        InvoiceId = factura.Id,
        NumarFactura = factura.NumarFactura,
        Factura = factura,
        NumarChitanta = numarChitanta,
        DataChitanta = DateTime.Now,
        Suma = dto.Suma,
        Moneda = dto.Moneda
      };

      _context.Receipts.Add(chitanta);
      await _context.SaveChangesAsync();

      return new ReceiptResponseDto
      {
        NumarChitanta = chitanta.NumarChitanta,
        DataChitanta = chitanta.DataChitanta,
        Suma = chitanta.Suma,
        Moneda = chitanta.Moneda,
        NumarFactura = factura.NumarFactura,
        DataFactura = factura.DataEmitere ?? DateTime.MinValue,
        ClientType = factura.ClientType,
        ClientId = factura.ClientId,
        CNP = factura.ClientType == "PF" ? factura.CNP : null,
        CUI = factura.ClientType == "PJ" ? factura.CUI : null,
        ClientName = factura.ClientName,
        AdresaClient = factura.AdresaClient
      };
    }




    public async Task<Receipt> GetReceiptByNumarChitantaAsync(string numarChitanta)
    {
      var chitanta = await _context.Receipts
          .Include(r => r.Factura)  // Încarcă și factura asociată
          .FirstOrDefaultAsync(r => r.NumarChitanta == numarChitanta);

      if (chitanta == null)
      {
        throw new Exception($"Chitanța cu numărul {numarChitanta} nu a fost găsită.");
      }

      return chitanta;
   }

    public async Task<IEnumerable<ReceiptResponseDto>> GetAllReceiptsAsync()
    {
     var chitante = await _context.Receipts
         .Include(r => r.Factura)
          .ToListAsync();

     return chitante.Select(ch => new ReceiptResponseDto
      {
        NumarChitanta = ch.NumarChitanta,
        DataChitanta = ch.DataChitanta,
        Suma = ch.Suma,
        Moneda = ch.Moneda,
        NumarFactura = ch.Factura.NumarFactura,
       DataFactura = ch.Factura.DataEmitere ?? DateTime.MinValue,
       ClientType = ch.Factura.ClientType,
        ClientId = ch.Factura.ClientId,
        CNP = ch.Factura.CNP,
        CUI = ch.Factura.CUI,
        ClientName = ch.Factura.ClientName,
        AdresaClient = ch.Factura.AdresaClient
      });
    }
  }
}
