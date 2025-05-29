namespace LawProject.DTO
{
  public class RaportDto
  {
    public int Id { get; set; }
    public string LawyerName { get; set; }
    public int LawyerId { get; set; }
    public DateTime DataRaport { get; set; }
    public double OreDeplasare { get; set; }
    public double? OreStudiu { get; set; }
    public double OreInstanta { get; set; }
    public double OreAudieri { get; set; }
    public double OreConsultante { get; set; }
    public double OreAlteActivitati { get; set; }

    public DateTime Date { get; set; }

    public List<RaportTaskDto>? TaskuriLucrate { get; set; }

    public List<RaportTaskDto>? Taskuri { get; set; }
    public List<RaportStudiuDosarDto>? StudiiPeDosar { get; set; }
  }
}
