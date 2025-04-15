namespace LawProject.DTO
{
  public class EventADTO
  {
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan Time { get; set; }
    public string Description { get; set; }

    public string EventType { get; set; } = "A";


  }
}
