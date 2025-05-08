namespace LawProject.Models
{
  public class Receipt
  {
    public int Id { get; set; }
    public string NumarChitanta { get; set; } 
    public DateTime DataChitanta { get; set; } = DateTime.Now;

    public decimal Suma { get; set; }
    public string Moneda { get; set; }


    // Relație cu factura

    public int InvoiceId { get; set; }
    public string NumarFactura { get; set; }
    public Invoice Factura { get; set; } 
  
}
}
