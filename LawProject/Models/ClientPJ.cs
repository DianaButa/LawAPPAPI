using System.ComponentModel.DataAnnotations;

namespace LawProject.Models
{
  public class ClientPJ
  {
    [Key] public int Id { get; set; }

    public string CompanyName { get; set; } = string.Empty;

    public string CUI { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;
    public string ClientType { get; set; } = "PJ"; // Always "PJ" for this type

    public virtual ICollection<ClientPJFile> ClientPJFiles { get; set; } = new List<ClientPJFile>();

    //public ICollection<EventA> EventsA { get; set; }

    //public ICollection<EventA> EventsC { get; set; }
  }
}
