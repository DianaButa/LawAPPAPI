namespace LawProject.DTO
{
  public class RaportGeneralDto
  {
    public string TipRaport { get; set; } // "DailyEvent" sau "Raport"

    // Campuri din DailyEvent
    public int? DailyEventId { get; set; }
    public string FileNumber { get; set; }
    public DateTime? Date { get; set; }
    public string Institutie { get; set; }
    public string Descriere { get; set; }
    public string ClientName { get; set; }
    public int? LawyerId { get; set; }
    public string LawyerName { get; set; }
    public string AllocatedHours { get; set; }
    public bool? IsCompleted { get; set; }
    public string EventType { get; set; }

    // Campuri din Raport
    public int? RaportId { get; set; }
    public string ClientType { get; set; }
    public int? ClientId { get; set; }
    public int? WorkTaskId { get; set; }
    public List<string> TaskuriLucrate { get; set; }
    public double OreDeplasare { get; set; }
    public double OreStudiu { get; set; }
  }
}
