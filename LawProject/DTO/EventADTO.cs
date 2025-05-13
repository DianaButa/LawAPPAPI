using System.ComponentModel.DataAnnotations;

namespace LawProject.DTO
{
  public class EventADTO
  {
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Time { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "ClientId este obligatoriu.")]
    public int ClientId { get; set; }

    [Required(ErrorMessage = "ClientType este obligatoriu. (PF sau PJ)")]
    public string ClientType { get; set; }

    public string? ClientName { get; set; } 

    public int? FileId { get; set; }

    [Required(ErrorMessage = "FileNumber este obligatoriu.")]
    public string? FileNumber { get; set; }

    [Required(ErrorMessage = "LawyerId este obligatoriu.")]
    public int LawyerId { get; set; }

    public string? LawyerName { get; set; } 

    public string EventType { get; set; } = "A";


    public string? Color { get; set; }
    public bool IsReported { get; set; }

  }
}
