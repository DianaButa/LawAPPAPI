namespace LawProject.Models
{
  public class Invoice
  {
    public int Id { get; set; }

    // Câmpuri obligatorii (non-nullable)
    public int ClientId { get; set; }
    public string ClientType { get; set; }  // "PF" sau "PJ"
    public string ClientName { get; set; }
    public string AdresaClient { get; set; }  
    public decimal SumaFinala { get; set; }  

    // Câmpuri care pot fi `nullable` (opționale)
    public string? CNP { get; set; }  
    public string? CUI { get; set; }  
    public int? FileId { get; set; }  
    public string? FileNumber { get; set; } 
    public string? Denumire { get; set; }  
    public int Cantitate { get; set; } 
    public decimal PretUnitar { get; set; } 
    public decimal TVAProcent { get; set; } 
    public string? Moneda { get; set; }  

  
    public DateTime? DataEmitere { get; set; }  
    public DateTime? DataScadenta { get; set; }  

    public string? NumarFactura { get; set; }


    public virtual List<POS> POSs { get; set; } = new();
    public virtual List<Receipt> Receipts { get; set; } = new();

    public bool IsCanceled { get; set; } = false;
    public bool IsStorned { get; set; } = false;
    public int? StornedInvoiceId { get; set; }

  }
}
