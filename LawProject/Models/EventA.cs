namespace LawProject.Models
{
  public class EventA
  {
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan Time { get; set; }
    public string Description { get; set; }
    public int ClientId { get; set; }
    public string ClientType { get; set; }

    public string FileNumber { get; set; }
    public int? FileId { get; set; }
    public int LawyerId { get; set; }

    public Lawyer Lawyer { get; set; }
    
    public string EventType { get; set; } = "A";

    public string Color { get; set; }

  }
}
