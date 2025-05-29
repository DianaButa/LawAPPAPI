namespace LawProject.Models
{
  public class RaportStudiuDosar
  {
    public int Id { get; set; }

    public int FileId { get; set; }
    public string FileNumber { get; set; }

    public double OreStudiu { get; set; }

    public int RaportId { get; set; }
    public virtual Raport Raport { get; set; }
  }
}
