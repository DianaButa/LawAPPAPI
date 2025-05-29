namespace LawProject.DTO
{
  public class AuthenticationResultDto
  {
    public string Token { get; set; }
    public string? UserId { get; set; }
    public bool IsValid { get; set; }
  }
}
