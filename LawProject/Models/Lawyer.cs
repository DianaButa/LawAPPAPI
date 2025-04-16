namespace LawProject.Models
{
  public class Lawyer
  {

    public int Id { get; set; }

    public string LawyerName { get; set; } = string.Empty;

    public string Color { get; set; }

    public virtual ICollection<LawyerFile> LawyerFiles { get; set; } = new List<LawyerFile>();

  }
}

