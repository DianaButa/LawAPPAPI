namespace LawProject.DTO
{


  public class DosarDTO
  {
    public string Numar { get; set; }                       // Numărul dosarului
    public string NumarVechi { get; set; }                   // Număr de dosar în format vechi
    public DateTime Data { get; set; }                       // Data dosarului
    public DateTime DataInitiala { get; set; }               // Data inițială a dosarului
    public string Departament { get; set; }                  // Departamentul (secţia)

    public List<ObiectDosarDTO> Obiect { get; set; }
    public List<DosarParteDTO> Parte { get; set; }

    public List<DosarSedintaDTO> Sedinte { get; set; }

    public List<DosarCaleAtacDTO> CaiAtac { get; set; }


    // Detalii despre obiectul dosarului
    public class ObiectDosarDTO
    {
      public string Descriere { get; set; }
    }


    public class DosarParteDTO
    {
      public string Nume { get; set; }
      public string CalitateaProcesualaCurenta { get; set; }
      public string CalitateaProcesualaAnterioara { get; set; }
      public DateTime Data { get; set; }
    }

    public class DosarSedintaDTO
    {
      public DateTime Data { get; set; }
      public string Ora { get; set; }
      public string Complet { get; set; }
      public string NumărDocument { get; set; }
      public DateTime DataDocument { get; set; }
      public string TipDocument { get; set; }
      public string Solutie { get; set; }
      public string DetaliiSolutie { get; set; }
    }


    // Detalii despre căile de atac
    public class DosarCaleAtacDTO
    {
      public DateTime DataDeclarare { get; set; }
      public string ParteDeclaratoare { get; set; }
      public string TipCaleAtac { get; set; }
    }
    // Detalii despre ședințele de judecată
    public class SedintaDTO
    {
      public string Departament { get; set; }
      public string Complet { get; set; }
      public DateTime Data { get; set; }
      public string Ora { get; set; }

      // Detalii despre dosarele din cadrul unei ședințe
      public class SedintaDosarDTO
      {
        public string Numar { get; set; }
        public DateTime Data { get; set; }
        public string Ora { get; set; }


      }
    }
  }
}
