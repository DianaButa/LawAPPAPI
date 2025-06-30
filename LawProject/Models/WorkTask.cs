namespace LawProject.Models
{
  public class WorkTask
  {
    public int Id { get; set; }

    // Asociere cu avocatul
    public int LawyerId { get; set; }

    public string LawyerName { get; set; } = string.Empty;

    public Lawyer Lawyer { get; set; }

    // Datele de început și de sfârșit (termen limită)
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    // Statusul task‑ului: "open" sau "closed"
    public string Status { get; set; }

    // Date despre client (PF sau PJ)


    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string ClientType { get; set; }

    public string FileNumber { get; set; }
    public int? FileId { get; set; }

    // Titlu (camp required)
    public string Title { get; set; }

    // Câmpuri opționale
    public string Description { get; set; }
    public string Comment { get; set; }

    // Orele lucrate (se completează la închiderea task‑ului)
    public double? HoursWorked { get; set; }
  }
}
