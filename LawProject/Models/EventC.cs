namespace LawProject.Models
{
  public class EventC
  {
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan Time { get; set; }
    public string Description { get; set; }

    public string EventType { get; set; } = "C";
  }
}
