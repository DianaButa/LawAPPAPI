using System.ComponentModel.DataAnnotations;

namespace LawProject.Models
{
  public class ClientPF
  {
    [Key] public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string CNP { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string ClientType { get; set; } = "PF"; // Always "PF" for this type

    //public ICollection<EventA> EventsA { get; set; }

    //public ICollection<EventA> EventsC { get; set; }

    public virtual ICollection<ClientPFFile> ClientPFFiles { get; set; } = new List<ClientPFFile>() {
    };
  }
}

