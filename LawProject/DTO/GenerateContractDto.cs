namespace LawProject.DTO
{
  public class GenerateContractDto
  {
    public int Id { get; set; }

    public int ClientId { get; set; }
    public string ClientType { get; set; }

    public string? ClientName { get; set; }

    public string Onorariu { get; set; }

    public string Scadenta { get; set; }

    public string Obiect { get; set; }
  }
}
