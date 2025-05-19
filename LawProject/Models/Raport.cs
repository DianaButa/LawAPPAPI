using System.Globalization;

namespace LawProject.Models
{
  public class Raport
  {

    public int Id { get; set; }

    public int LawyerId { get; set; }

    public string? LawyerName { get; set; }

    public virtual Lawyer Lawyer { get; set; }

    public string? ClientType { get; set; }
    public string? ClientName { get; set; }
    public int? ClientId { get; set; }
    public virtual ClientPF? ClientPF { get; set; }
    public virtual ClientPJ? ClientPJ { get; set; }

    public int? FileId { get; set; }
    public string? FileNumber { get; set; }

    public DateTime DataRaport { get; set; }
    public DateTime Date { get; set; }

    public int? WorkTaskId { get; set; }

    public virtual WorkTask? WorkTask { get; set; }

    public double OreDeplasare { get; set; }
    public double OreStudiu { get; set; }

    public virtual ICollection<RaportTask> TaskuriLucrate { get; set; }

  }
}
