using LawProject.DTO;

namespace LawProject.Service.AccountService
{
  public interface IAccountService
  {
    Task<AuthenticationResultDto> Register(UserDto user);
    Task<AuthenticationResultDto> Login(UserDto model);
    Task<string> ForgotPassword(ForgotPasswordDto forgot);
    Task<string> ResetPassword(ResetPasswordDto reset);

    Task<UserDto> DeleteUser(string email);

  }
}
