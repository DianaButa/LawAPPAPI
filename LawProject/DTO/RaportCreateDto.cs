namespace LawProject.DTO
{
  public class RaportCreateDto
  {

    public int LawyerId { get; set; }
    public DateTime DataRaport { get; set; }

    public int? ClientId { get; set; }
    public string? ClientName { get; set; }
    public string? ClientType {  get; set; }
    public int? FileId { get; set; }

    public string? FileNumber { get; set; }

    public double OreDeplasare { get; set; }
    public double OreStudiu { get; set; }

    public List<RaportTaskDto> Taskuri { get; set; } = new();
  }
}
