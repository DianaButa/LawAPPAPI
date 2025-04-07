using System.ComponentModel.DataAnnotations;

namespace LawProject.Models
{
  public class ClientPJFile
  {
    [Key]
    public int ClientPJId { get; set; }
    public virtual ClientPJ ClientPJ { get; set; }

    [Key]
    public int MyFileId { get; set; }
    public virtual MyFile MyFile { get; set; }
  }
}
