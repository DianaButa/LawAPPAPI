namespace LawProject.Models
{
  public class Contract
  {

    public int Id { get; set; }

    public int ClientId { get; set; }

    public string? ClientName { get; set; }
    public string ClientType { get; set; }

    public string? ClientAdress { get; set; }

    public string? CNP { get; set; }

    public string? CUI { get; set; }

    public string Onorariu { get; set; }

    public string Scadenta {  get; set; }

    public string Obiect {  get; set; }


  }
}
