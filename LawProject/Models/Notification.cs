using System.ComponentModel.DataAnnotations;

namespace LawProject.Models
{
  public class Notification
  {
    [Key]
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; }

    public string Type { get; set; }

    public string FileNumber { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public string Details { get; set; } = string.Empty;
    public int UserId { get; set; }

    public string? Source { get; set; }
  }
}
