using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace LawProject.DTO
{
  public class CreateFileDto
  {
    public int Id { get; set; }

    [Required(ErrorMessage = "Numărul fișierului este obligatoriu")]
    public string FileNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "ID-ul clientului este obligatoriu")]
    public int ClientId { get; set; }
    public string ClientType { get; set; }
    public string? ClientName { get; set; }
    public string? Source { get; set; }

    public string? Details { get; set; } = string.Empty;

    public string? Parola { get; set; }=string.Empty;
    public string Moneda { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Instanta { get; set; } = string.Empty;
    public string Onorariu {  get; set; } = string.Empty;
    public string OnorariuRestant { get; set; } = string.Empty;

    public string? CuvantCheie { get; set; } = string.Empty;

    public string Outcome {  get; set; } = string.Empty; 
    public string Delegatie { get; set; } = string.Empty;
    public string NumarContract { get; set; } = string.Empty;

    public DateTime? DataScadenta { get; set; }


    [Required(ErrorMessage = "Tipul dosarului este obligatoriu")]
    [RegularExpression("^(civil|penal)$", ErrorMessage = "Tipul dosarului trebuie să fie 'civil' sau 'penal'")]
    public string TipDosar { get; set; } = string.Empty;

    public int? LawyerId { get; set; }
    public string LawyerName { get; set; } = string.Empty;

    public string LawyerEmail {  get; set; } = string.Empty;
  }
}
