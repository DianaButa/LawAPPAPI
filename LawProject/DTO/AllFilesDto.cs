
using Newtonsoft.Json;

namespace LawProject.DTO
{
  public class AllFilesDto
  {
    // Informații din baza de date (CreateFileDTO)
    public int Id { get; set; }
    public string FileNumber { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;

    public int ClientId { get; set; }
    public string ClientType { get; set; }
    public string Details { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string TipDosar { get; set; } = string.Empty;

    public string Status {  get; set; } = string.Empty;

    public string Onorariu { get; set; } = string.Empty;

    public string? Source { get; set; } = string.Empty;
    public string? Parola { get; set; } = string.Empty;
    public string CuvantCheie { get; set; } = string.Empty;

    public string OutCome { get; set; } = string.Empty;

    public string OnorariuRestant {  get; set; } =string.Empty;

    public string NumarContract { get; set; } = string.Empty;

    public string Delegatie { get; set; } = string.Empty;

    public DateTime? DataScadenta { get; set; }

    public string Instanta {  get; set; } = string.Empty;
    public int? LawyerId {  get; set; } 
    public string LawyerName { get; set; } = string.Empty;

    // Informații din SOAP (FileDetailsDTO)
    public string Numar { get; set; }
    public string NumarVechi { get; set; }
    public DateTime Data { get; set; }
    public string Institutie { get; set; }

    public string ObiectDosar {  get; set; } 
    public string Moneda { get; set; }
    public string Departament { get; set; }
    public string CategorieCaz { get; set; }
    public string StadiuProcesual { get; set; }


    public string OraTermen { get; set; } = string.Empty;
    public string Solutie { get; set; } = string.Empty;
    public string DetaliiSolutie { get; set; } = string.Empty;
    public List<ParteDTO> Parti { get; set; }
    public List<SedintaDTO> Sedinte { get; set; }
    public List<CaleAtacDTO> CaiAtac { get; set; }

    public List<TermeneDTO> Termene { get; set; }

    public List<SedintaIccjDto> SedinteIccj { get; set; } = new List<SedintaIccjDto>();


  }
}
