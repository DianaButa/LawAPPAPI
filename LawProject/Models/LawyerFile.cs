using System.ComponentModel.DataAnnotations;

namespace LawProject.Models
{
  public class LawyerFile
  {
    [Key]
    public int LawyerId { get; set; }
    public virtual Lawyer Lawyer { get; set; }

    public int FileId { get; set; }
    public virtual MyFile File { get; set; }
  }
}
