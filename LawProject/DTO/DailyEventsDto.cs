namespace LawProject.DTO
{
  public class DailyEventsDto
  {
    public int Id { get; set; }

    public string? FileNumber { get; set; }

    public DateTime Date { get; set; }

    public int? ScheduledEventId { get; set; }

    public int? EventCId { get; set; }

    public int? EventAId { get; set; }

    public string? Institutie { get; set; }
    public string? Descriere { get; set; }

    public string ClientName { get; set; }

    public int LawyerId { get; set; }
    public string? LawyerName { get; set; }

    public string AllocatedHours { get; set; }

    public bool IsCompleted { get; set; }
    public string EventType { get;  set; }
  }
}
