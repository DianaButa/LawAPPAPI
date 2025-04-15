using System.ComponentModel.DataAnnotations;

namespace LawProject.DTO
{
  public class CreateTaskDto
  {
    [Required(ErrorMessage = "LawyerId este obligatoriu.")]
    public int LawyerId { get; set; }

    [Required(ErrorMessage = "Data de început este obligatorie.")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "Data de sfârșit/termenul limită este obligatorie.")]
    public DateTime EndDate { get; set; }

    [Required(ErrorMessage = "ClientId este obligatoriu.")]
    public int ClientId { get; set; }

    [Required(ErrorMessage = "ClientType este obligatoriu. (PF sau PJ)")]
    public string ClientType { get; set; }

    public int? FileId { get; set; }

    [Required(ErrorMessage = "FileNumber este obligatoriu.")]
    public string FileNumber { get; set; } 

    [Required(ErrorMessage = "Titlul este obligatoriu.")]
    public string Title { get; set; }

    // Opțional
    public string Description { get; set; }
    public string Comment { get; set; }
  }
}
