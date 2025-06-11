namespace LawProject.Models
{
  public class Lawyer
  {

    public int Id { get; set; }

    public string LawyerName { get; set; } = string.Empty;

    public string Color { get; set; }

    public string Email {  get; set; }= string.Empty;

    public virtual ICollection<LawyerFile> LawyerFiles { get; set; } = new List<LawyerFile>();
    public int UserId { get; set; }
    public virtual User User { get; set; }

  }
}

