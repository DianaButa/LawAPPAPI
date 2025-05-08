using System.ComponentModel.DataAnnotations;

namespace LawProject.DTO
{
  public class InvoiceDto

   
  {

    public int Id { get; set; }
    public string ClientType { get; set; }  // "PF" sau "PJ"
    public int ClientId { get; set; }

    public string CNP { get; set; }  
    public string CUI { get; set; } 
    public string ClientName { get; set; }  
    public string AdresaClient { get; set; }  
    public string? Denumire { get; set; } 
    public int Cantitate { get; set; } 
    public decimal PretUnitar { get; set; }
    public decimal TVAProcent { get; set; }  

    public decimal SumaFinala { get; set; }  
    public string? Moneda { get; set; }  

    // Câmpuri de tip `nullable`
    public DateTime? DataEmitere { get; set; } 
    public DateTime? DataScadenta { get; set; } 
    public string? NumarFactura { get; set; } 

    public int? FileId { get; set; }
    public string? FileNumber { get; set; }

    public decimal SumaPlatita { get; set; }
    public decimal SumaNeachitata { get; set; }


  }
}
