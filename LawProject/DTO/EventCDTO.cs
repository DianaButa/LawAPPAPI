using System.ComponentModel.DataAnnotations;

namespace LawProject.DTO
{
  public class EventCDTO
  {
     public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Time { get; set; } = string.Empty;

    public string Description { get; set; }

    [Required(ErrorMessage = "LawyerId este obligatoriu.")]
    public int LawyerId { get; set; }

    public string LawyerName { get; set; } = string.Empty;


    public int? ClientId { get; set; }

    public string? ClientName { get; set; }


    public string? ClientType { get; set; }

    public int? FileId { get; set; }
    public bool IsReported { get; set; }


    public string? FileNumber { get; set; }
    public string EventType { get; set; } = "C";

    public string? Color { get; set; }
  }
}
