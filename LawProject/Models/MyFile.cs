using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LawProject.Models
{
  public class MyFile
  {
    [Key]
    public int Id { get; set; }

    [Required]
    public string FileNumber { get; set; }

    [Required]
    public string ClientName { get; set; }

    public int ClientId { get; set; }

    public int? ClientPFId { get; set; }
    public virtual ClientPF ClientPF { get; set; }

    public int? ClientPJId { get; set; }
    public virtual ClientPJ ClientPJ { get; set; }

    public string Details { get; set; } = string.Empty;

    public string TipDosar { get; set; } = string.Empty;

    public string CuloareCalendar { get; set; }

    public int? LawyerId { get; set; }  // LawyerId este nullable

    [ForeignKey("LawyerId")]
    public virtual Lawyer Lawyer { get; set; }

    public virtual List<Notes> Notes { get; set; } = new List<Notes>();

    // Colecțiile pentru ClientFile și LawyerFile
    public virtual ICollection<ClientPFFile> ClientPFFiles { get; set; } = new List<ClientPFFile>();
    public virtual ICollection<ClientPJFile> ClientPJFiles { get; set; } = new List<ClientPJFile>();

    public virtual ICollection<LawyerFile> LawyerFiles { get; set; } = new List<LawyerFile>();

    public string Stadiu { get; set; } = "Deschis";
  }
}
