namespace LawProject.DTO
{
  public class SoapDosar
  {
    public string numar { get; set; }
    public DateTime data { get; set; }
    public string institutie { get; set; }
    public string obiect { get; set; }
    public List<Sedinta> sedinte { get; set; }
  }

  public class Sedinta
  {
    public string complet { get; set; }
    public DateTime data { get; set; }
    public string ora { get; set; }
    public string solutie { get; set; }
    public string solutieSumar { get; set; }
    public DateTime? dataPronuntare { get; set; }
  }
}
