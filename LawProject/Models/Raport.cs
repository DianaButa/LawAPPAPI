using System.Globalization;

namespace LawProject.Models
{
  public class Raport
  {

    public int Id { get; set; }

    public int LawyerId { get; set; }

    public string? LawyerName { get; set; }

    public virtual Lawyer Lawyer { get; set; }


    public int? FileId { get; set; }
    public string? FileNumber { get; set; }

    public DateTime DataRaport { get; set; }
    public DateTime? Date { get; set; }

    public int? WorkTaskId { get; set; }

    public virtual WorkTask? WorkTask { get; set; }

    public double OreDeplasare { get; set; }
    public double OreStudiu { get; set; }

    public virtual ICollection<RaportStudiuDosar>? StudiiPeDosar { get; set; } = new List<RaportStudiuDosar>();


    public double OreInstanta {  get; set; }
    public double OreAudieri { get; set; }
    public double OreConsultante {  get; set; }

    public double OreAlteActivitati {  get; set; }
    public virtual ICollection<RaportTask>? TaskuriLucrate { get; set; }

  }
}
