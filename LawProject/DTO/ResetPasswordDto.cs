namespace LawProject.DTO
{
  public class ResetPasswordDto
  {
    public string Token { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
  }
}
