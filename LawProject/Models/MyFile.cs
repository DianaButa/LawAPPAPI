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

    [ForeignKey("ClientId")]
    public virtual Client Client { get; set; }

    public string Details { get; set; } = string.Empty.ToString();

    public string TipDosar { get; set; } = string.Empty.ToString();

    public string CuloareCalendar { get; set; }

    public virtual List<Notes> Notes { get; set; } = new List<Notes>();
  }
}
