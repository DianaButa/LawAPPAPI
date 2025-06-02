namespace LawProject.Models
{
  public class User
  {
    public int Id { get; set; }

    public string UserName { get; set; }= string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public bool IsValidated { get; set; } = false;

    public string? ResetToken { get; set; }
    public DateTime? ResetTokenExpiry { get; set; }

    public string Role { get; set; } = "User"; 

    public virtual Lawyer Lawyer { get; set; }


  }
}
