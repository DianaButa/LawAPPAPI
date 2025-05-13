using System.Text.Json.Serialization;

namespace LawProject.DTO
{
  public class FileDetailsIccjDto


  {

    public string FileNumber { get; set; }
    public string Details { get; set; }
    public string ClientName { get; set; }
    public string Numar { get; set; } // Numărul dosarului (număr unic de dosar în formatul [numar]/[identificator institutie]/[an])
    public string NumarVechi { get; set; } // Număr de dosar în format vechi (poate lipsi)
    public DateTime Data { get; set; } // Data dosarului (data creării dosarului la ICCJ)
    public DateTime DataInitiala { get; set; } // Data inițială a dosarului (data creării dosarului la prima instanță)
    public string Departament { get; set; } // Departamentul (secția)
    public string CategorieCaz { get; set; } // Categoria cazului (materia juridică)

    public string? Source { get; set; }
    public string StadiuProcesual { get; set; } // Stadiul procesual al dosarului (conform listei ICCJ)
    public string StadiuProcesualCombinat { get; set; } // Stadiul procesual combinat al dosarului (conform listei ICCJ)
    public string Obiect { get; set; } // Obiectul dosarului
    public List<ParteDTO> Parti { get; set; } // Lista de părți din dosar
    //public List<SedintaIccjDto> Sedinte { get; set; } // Lista de termene (sedințe) din dosar
    public List<CaleAtacDTO> CaiAtac { get; set; } // Lista de căi de atac

    [JsonPropertyName("sedinte")]
    public List<DosarTermeneDto> Sedinte { get; set; } 
  }

  public class DosarTermeneDto
  {
    public DateTime Data { get; set; }               // Data ședinței
    public string Ora { get; set; }                  // Ora ședinței
    public string Complet { get; set; }              // Completul
    public string NumarDocument { get; set; }        // Numărul documentului
    public DateTime DataDocument { get; set; }       // Data documentului
    public string TipDocument { get; set; }          // Tipul documentului
    public string Solutie { get; set; }              // Soluția
    public string DetaliiSolutie { get; set; }       // Detalii soluție

    public List<SedintaDosarIccjDto> Dosare { get; set; }
  }


  public class ParteIccjDTO
  {
    public string Nume { get; set; } // Numele părții din dosar
    public string CalitateParte { get; set; } // Calitatea procesuală a părții (ex: reclamant, pârât, etc.)
  }

  public class SedintaIccjDto
  {
    public string Departament { get; set; } = string.Empty; // Secția (departamentul)
    public string Complet { get; set; } = string.Empty; // Numele completului de judecată
    public DateTime Data { get; set; } // Data ședinței
    public string Ora { get; set; } = string.Empty; // Ora ședinței
    public List<SedintaDosarIccjDto> Dosare { get; set; } = new List<SedintaDosarIccjDto>(); // Lista dosarelor aferente ședinței
  }

  public class SedintaDosarIccjDto
  {
    public string Numar { get; set; } = string.Empty; // Numărul dosarului
    public DateTime Data { get; set; } // Data ședinței din dosar
    public string Ora { get; set; } = string.Empty; // Ora ședinței din dosar
    public string CategorieCaz { get; set; } = string.Empty; // Categorie caz (materia juridică)
    public string StadiuProcesual { get; set; } = string.Empty; // Stadiul procesual al dosarului
  }


  public class CaleAtacIccjDTO
  {
    public DateTime DataDeclarare { get; set; } // Data declarării căii de atac
    public string ParteDeclaratoare { get; set; } // Partea care declară calea de atac
    public string TipCaleAtac { get; set; } // Tipul căii de atac (ex: apel, recurs)
  }
}

