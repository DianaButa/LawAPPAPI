using LawProject.DTO;

namespace LawProject.Models
{
  public class ScheduledEvent
  {
    public int Id { get; set; }
    public string FileNumber { get; set; }
    public DateTime StartTime { get; set; }



    public DateTime EndTime { get; set; }
    public string TipDosar { get; set; } 
    public string Description { get; set; }
    public string ClientName { get; set; }
    public string Color { get; set; } 
    public bool IsReported { get; set; }

    public string EventType { get; set; } = "S";


  }
}
