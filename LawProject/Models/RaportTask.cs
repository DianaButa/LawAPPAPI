namespace LawProject.Models
{
  public class RaportTask
  {

    public int Id { get; set; }

    public int RaportId { get; set; }
    public virtual Raport Raport { get; set; }

    public int WorkTaskId { get; set; }
    public virtual WorkTask WorkTask { get; set; }

    public double OreLucrate { get; set; } 
  }
}
