namespace LawProject.DTO
{
  public class DelegatieDto
  {
    public string ClientType { get; set; } // PF / PJ
    public int ClientId { get; set; }
    public int FileId { get; set; } // Id-ul dosarului (file) ales
    public string Institutie { get; set; }
    public string Activitate { get; set; }
  }
}
