namespace LawProject.DTO
{
  public class ReceiptResponseDto
  {
    public string NumarChitanta { get; set; }
    public DateTime DataChitanta { get; set; }
    public decimal Suma { get; set; }

    public string NumarFactura { get; set; }
    public DateTime DataFactura { get; set; }

    public string ClientType { get; set; } // "PF" / "PJ"
    public int ClientId { get; set; }

    public string CNP { get; set; }
    public string CUI { get; set; }
    public string ClientName { get; set; }
    public string AdresaClient { get; set; }
  }
}
