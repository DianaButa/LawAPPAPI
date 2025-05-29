namespace LawProject.Models
{
  public class POS
  {
    public int Id { get; set; }
    public DateTime DataPOS { get; set; } = DateTime.Now;
    public string NumarIncasare { get; set; }


    public decimal Suma { get; set; }
    public string Moneda { get; set; }


    // Relație cu factura

    public int InvoiceId { get; set; }
    public string NumarFactura { get; set; }
    public Invoice Factura { get; set; } 
  }
}
