using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LawProject.Models
{
  public class Notes
  {
    [Key]
    public int Id { get; set; }

    [Required]
    public string Text { get; set; } = string.Empty;


    [ForeignKey("Dosar")]
    public int DosarId { get; set; }

    public virtual MyFile? Dosar { get; set; }

  }
}
