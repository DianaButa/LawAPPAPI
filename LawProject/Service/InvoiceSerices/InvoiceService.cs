using LawProject.Database;
using LawProject.DTO;
using LawProject.Models;
using LawProject.Service.ClientService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.ServiceModel;

namespace LawProject.Service.InvoiceSerices
{
  public class InvoiceService : IInvoiceService
  {
    private readonly IClientService _clientService;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<InvoiceService> _logger;

    public InvoiceService(IClientService clientService, ApplicationDbContext dbContext, ILogger<InvoiceService> logger)
    {
      _clientService = clientService;
      _dbContext = dbContext;
      _logger = logger;
    }

    public async Task<Invoice> GenereazaFacturaAsync(InvoiceDto dto)
    {
      var clientType = dto.ClientType.ToUpper();
      string clientName = string.Empty;
      string adresaClient = string.Empty;
      string cnp = null;
      string cui = null;

      switch (clientType)
      {
        case "PF":
          var clientPF = await _dbContext.ClientPFs.FirstOrDefaultAsync(c => c.Id == dto.ClientId);
          if (clientPF == null)
            throw new ArgumentException($"Clientul fizic cu ID {dto.ClientId} nu a fost găsit.");

          clientName = $"{clientPF.FirstName} {clientPF.LastName}";
          adresaClient = clientPF.Address;
          cnp = clientPF.CNP;
          break;

        case "PJ":
          var clientPJ = await _dbContext.ClientPJs.FirstOrDefaultAsync(c => c.Id == dto.ClientId);
          if (clientPJ == null)
            throw new ArgumentException($"Clientul juridic cu ID {dto.ClientId} nu a fost găsit.");

          clientName = clientPJ.CompanyName;
          adresaClient = clientPJ.Address;
          cui = clientPJ.CUI;
          break;

        default:
          throw new ArgumentException($"ClientType invalid: {dto.ClientType}. Se acceptă doar PF sau PJ.");
      }

      var invoice = new Invoice
      {
        ClientId = dto.ClientId,
        ClientType = clientType,
        ClientName = clientName,
        AdresaClient = adresaClient,
        CNP = cnp,  
        CUI = cui,

        Denumire = dto.Denumire,
        Cantitate = dto.Cantitate,
        PretUnitar = dto.PretUnitar,
        TVAProcent = dto.TVAProcent,
        Moneda = dto.Moneda,
        FileId = dto.FileId,
        FileNumber = dto.FileNumber,
        DataEmitere = DateTime.Now,
        DataScadenta = dto.DataScadenta ?? DateTime.Now.AddDays(30),
        NumarFactura = GenerateInvoiceNumber(),
        SumaFinala = dto.SumaFinala
      };

      _dbContext.Invoices.Add(invoice);

      if (dto.FileId.HasValue)
      {
        var file = await _dbContext.Files.FirstOrDefaultAsync(f => f.Id == dto.FileId.Value);
        if (file != null)
        {
          var culture = System.Globalization.CultureInfo.InvariantCulture;

          // Îndepărtăm cuvintele "Lei" sau orice alt text din OnorariuRestant
          string onorariuRestantStr = file.OnorariuRestant?.Replace("Lei", "").Trim();

          // Verificăm dacă OnorariuRestant este un număr valid
          if (decimal.TryParse(onorariuRestantStr, System.Globalization.NumberStyles.Any, culture, out decimal restant) &&
              decimal.TryParse(dto.SumaFinala.ToString(), out decimal sumaFactura))
          {
            decimal nouRestant = restant - sumaFactura;

            if (nouRestant <= 0)
            {
              file.OnorariuRestant = "0";
              file.Status = "achitat";
            }
            else
            {
              file.OnorariuRestant = nouRestant.ToString("0.##", culture);
            }

            _dbContext.Files.Update(file);
          }
          else
          {
            throw new InvalidOperationException("OnorariuRestant nu este un număr valid.");
          }
        }
      }

      await _dbContext.SaveChangesAsync();
      return invoice;
    }

    private string GenerateInvoiceNumber()
    {
      // Obținem anul curent
      var year = DateTime.Now.Year.ToString();

      // Numărăm facturile deja emise în anul curent
      var invoiceCount = _dbContext.Invoices
          .Count(i => i.DataEmitere.HasValue && i.DataEmitere.Value.Year == DateTime.Now.Year) + 1;

      // Generăm numărul facturii în formatul: INV-YYYY-XXXX (unde YYYY este anul, iar XXXX este numărul facturii)
      return $"DAAF-{invoiceCount:D4}";
    }


    public async Task<List<InvoiceDto>> GetAllInvoicesAsync()
    {
      var invoices = await _dbContext.Invoices
          .Include(i => i.Receipts) // important!
          .ToListAsync();

      var result = invoices.Select(invoice =>
      {
        var sumaPlatita = invoice.Receipts.Sum(r => r.Suma);

        return new InvoiceDto
        {
          Id = invoice.Id,
          ClientName = invoice.ClientName ?? string.Empty,
          AdresaClient = invoice.AdresaClient ?? string.Empty,
          ClientType = invoice.ClientType ?? string.Empty,
          FileNumber= invoice.FileNumber ?? string.Empty,
          Denumire = invoice.Denumire ?? string.Empty,
          Cantitate = invoice.Cantitate,
          PretUnitar = invoice.PretUnitar,
          TVAProcent = invoice.TVAProcent,
          SumaFinala = invoice.SumaFinala,
          Moneda = invoice.Moneda ?? "RON",
          DataEmitere = invoice.DataEmitere ?? DateTime.MinValue,
          DataScadenta = invoice.DataScadenta ?? DateTime.MinValue,
          NumarFactura = invoice.NumarFactura ?? string.Empty,

          SumaPlatita = sumaPlatita
          // StatusPlata se calculează direct în DTO
        };
      }).ToList();

      return result;
    }


    public async Task<Invoice> GetInvoiceByIdAsync(int id)
    {
      // Găsim factura pe baza ID-ului
      return await _dbContext.Invoices
                           .FirstOrDefaultAsync(i => i.Id == id);
    }



    public async Task<IEnumerable<InvoiceDto>> GetInvoicesByDataScadentaAsync()
    {
      var azi = DateTime.Today;

      var facturi = await _dbContext.Invoices
          .Include(f => f.Receipts)
          .Where(f => f.DataScadenta >= azi)
          .ToListAsync();

      var restante = facturi
          .Select(f =>
          {
            var sumaPlatita = f.Receipts.Sum(r => r.Suma);
            var sumaNeachitata = f.SumaFinala - sumaPlatita;

            return new InvoiceDto
            {
              Id = f.Id,
              ClientId = f.ClientId,
              ClientType = f.ClientType,
              ClientName = f.ClientName,
              AdresaClient = f.AdresaClient,
              Denumire = f.Denumire,
              Cantitate = f.Cantitate,
              PretUnitar = f.PretUnitar,
              TVAProcent = f.TVAProcent,
              SumaFinala = f.SumaFinala,
              Moneda = f.Moneda,
              DataEmitere = f.DataEmitere ?? DateTime.MinValue,
              DataScadenta = f.DataScadenta ?? DateTime.MinValue,
              NumarFactura = f.NumarFactura,

              SumaPlatita = sumaPlatita,
              SumaNeachitata = sumaNeachitata
            };
          })
          .Where(f => f.SumaNeachitata > 0)
          .ToList();

      return restante;
    }




  }
}
