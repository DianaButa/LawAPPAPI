using System.ComponentModel.DataAnnotations;

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

    // ClientName is optional as it will be set automatically from the client's first and last name
    public string? ClientName { get; set; }

    public string Details { get; set; } = string.Empty;


    public string Status { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public string Instanta { get; set; } = string.Empty;

    public string Onorariu {  get; set; } = string.Empty;

    public string OnorariuRestant { get; set; } = string.Empty;

    public string Delegatie { get; set; } = string.Empty;
    public string NumarContract { get; set; } = string.Empty;

    public DateTime? DataScadenta { get; set; }


    [Required(ErrorMessage = "Tipul dosarului este obligatoriu")]
    [RegularExpression("^(civil|penal)$", ErrorMessage = "Tipul dosarului trebuie să fie 'civil' sau 'penal'")]
    public string TipDosar { get; set; } = string.Empty;

    public int? LawyerId { get; set; }
    public string LawyerName { get; set; } = string.Empty;
  }
}
