using System.Globalization;

namespace LawProject.Models
{
  public class Delegatie
  {
    public int Id { get; set; }

    public int ClientId { get; set; }

    public string? ClientName { get; set; }
    public string ClientType { get; set; }
    public string? FileId { get; set; }

    public string? FileNumarContract { get; set; }

    public string? FileDelegatie { get; set; }

    public string Activitate { get; set; }

    public string Institutie { get; set; }
    public virtual MyFile Files { get; set; }

    public virtual ClientPJ ClientPJ { get; set; }

    public virtual ClientPF ClientPF { get; set; }


  }
}
