using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawProject.Models
{
  public class ClientPFFile
  {
    [Key]
    public int ClientPFId { get; set; }
    public virtual ClientPF ClientPF { get; set; }

    [Key]
    public int MyFileId { get; set; }
    public virtual MyFile MyFile { get; set; }
  }
}
