using System.Drawing;

namespace LawProject.Models
{
  public class Cheltuieli
  {
    public int Id { get; set; }

    public string Categorie { get; set; }=string.Empty;

    public string Titlu { get; set; } = string.Empty;
    public DateTime? Data { get; set; }

    public decimal Valoare {  get; set; }
    public string? Moneda { get; set; }
    public string Descriere { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public int? UserId {  get; set; } 

    public User User { get; set; }

  }
}
