namespace LawProject.DTO
{
 
    public class RaportCreateDto
    {
      public int LawyerId { get; set; }
      public DateTime DataRaport { get; set; }

      public double OreDeplasare { get; set; }
      public double OreInstanta { get; set; }
      public double OreAudieri { get; set; }
      public double OreConsultante { get; set; }
      public double OreAlteActivitati { get; set; }

      // ✅ Taskuri multiple
      public List<RaportTaskDto>? Taskuri { get; set; } = new();

      // ✅ Ore studiu pentru dosare multiple
      public List<RaportStudiuDosarDto>? StudiiPeDosar { get; set; } = new();
    }

    public class RaportStudiuDosarDto
    {
      public int? FileId { get; set; }
      public string? FileNumber { get; set; }
      public double? OreStudiu { get; set; }
    }
  }

