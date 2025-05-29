using LawProject.Database;
using LawProject.DTO;
using LawProject.Models;
using LawProject.Service.ReceiptService;
using Microsoft.EntityFrameworkCore;

namespace LawProject.Service.POSService
{
  public class POSService : IPOSService
  {
    private readonly ApplicationDbContext _context;

    public POSService(ApplicationDbContext context)
    {
      _context = context;
    }

    public async Task<POSResponseDto> GenereazaIncasareAsync(POSCreateDto dto)
    {
      var factura = await _context.Invoices
          .FirstOrDefaultAsync(f => f.Id == dto.InvoiceId);

      if (factura == null)
      {
        throw new Exception("Factura nu a fost găsită.");
      }

      var numarIncasare = $"CH-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

      var incasare = new POS
      {
        InvoiceId = factura.Id,
        NumarFactura = factura.NumarFactura,
        Factura = factura,
        NumarIncasare = numarIncasare,
        DataPOS = DateTime.Now,
        Suma = dto.Suma,
        Moneda = dto.Moneda
      };

      _context.POSs.Add(incasare);
      await _context.SaveChangesAsync();


      return new POSResponseDto
      {
        NumarIncasare = incasare.NumarIncasare,
        DataPOS = incasare.DataPOS,
        Suma = incasare.Suma,
        Moneda = incasare.Moneda,
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




    public async Task<POS> GetPOSByNumarIncasareAsync(string numarIncasare)
    {
      var chitanta = await _context.POSs
          .Include(r => r.Factura)  // Încarcă și factura asociată
          .FirstOrDefaultAsync(r => r.NumarIncasare == numarIncasare);

      if (chitanta == null)
      {
        throw new Exception($"Chitanța cu numărul {numarIncasare} nu a fost găsită.");
      }

      return chitanta;
    }

    public async Task<IEnumerable<POSResponseDto>> GetAllPOSsAsync()
    {
      var chitante = await _context.POSs
          .Include(r => r.Factura)
           .ToListAsync();

      return chitante.Select(ch => new POSResponseDto
      {
        NumarIncasare = ch.NumarIncasare,
        DataPOS = ch.DataPOS,
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
