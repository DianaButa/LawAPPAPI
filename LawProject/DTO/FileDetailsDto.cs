namespace LawProject.DTO
{
  public class FileDetailsDto
  {
    public string Numar { get; set; }
    public string NumarVechi { get; set; }
    public DateTime Data { get; set; }
    public string Institutie { get; set; }
    public string Departament { get; set; }
    public string CategorieCaz { get; set; }
    public string StadiuProcesual { get; set; }
    public List<ParteDTO> Parti { get; set; }
    public List<SedintaDTO> Sedinte { get; set; }
    public List<CaleAtacDTO> CaiAtac { get; set; }
  }

  public class ParteDTO
  {
    public string Nume { get; set; }
    public string CalitateParte { get; set; }
  }

  public class SedintaDTO
  {
    public string Complet { get; set; }
    public DateTime Data { get; set; }
    public string Ora { get; set; }
    public string Solutie { get; set; }
    public string SolutieSumar { get; set; }
    public DateTime? DataPronuntare { get; set; }
    public string DocumentSedinta { get; set; }
    public string NumarDocument { get; set; }
    public DateTime? DataDocument { get; set; }
  }

  public class CaleAtacDTO
  {
    public DateTime DataDeclarare { get; set; }
    public string ParteDeclaratoare { get; set; }
    public string TipCaleAtac { get; set; }
  }
}


