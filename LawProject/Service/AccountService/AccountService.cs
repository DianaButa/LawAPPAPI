using LawProject.DTO;
using LawProject.Database;
using LawProject.Service.EmailService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using LawProject.Models;

namespace LawProject.Service.AccountService
{
  public class AccountService : IAccountService
  {
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly ApplicationDbContext _context;

    public AccountService(IConfiguration configuration, IEmailService emailService, ApplicationDbContext context)
    {
      _configuration = configuration;
      _emailService = emailService;
      _context = context;
    }

    public async Task<AuthenticationResultDto> Register(UserDto userDto)
    {
      var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userDto.Email);
      if (existingUser != null)
        throw new Exception("User already exists.");

      var hashedPassword = HashPassword(userDto.Password);

      var newUser = new User
      {
        UserName = userDto.UserName,
        Email = userDto.Email,
        PasswordHash = hashedPassword,
        IsValidated = true ,
        Role = userDto.Role ?? "User"

      };

      _context.Users.Add(newUser);
      await _context.SaveChangesAsync();

      var token = GenerateJwtToken(newUser);

      return new AuthenticationResultDto
      {
        Token = token,
        UserId = newUser.Id.ToString(),
        IsValid = true
      };
    }

    public async Task<AuthenticationResultDto> Login(UserDto userDto)
    {
      var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userDto.Email);
      if (user == null)
        throw new Exception("User not found.");

      if (!VerifyPassword(userDto.Password, user.PasswordHash))
        throw new Exception("Invalid credentials.");

      if (!user.IsValidated)
        throw new Exception("Email not validated.");

      var token = GenerateJwtToken(user);

      return new AuthenticationResultDto
      {
        Token = token,
        UserId = user.Id.ToString(),
        IsValid = true
      };
    }

    private string GenerateJwtToken(User user)
    {
      var claims = new[]
      {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

      var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
      var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

      var duration = int.Parse(_configuration["JWT:TokenDurationMinutes"]);
      var token = new JwtSecurityToken(
          _configuration["JWT:Issuer"],
          _configuration["JWT:Audience"],
          claims,
            expires: DateTime.Now.AddMinutes(duration),
          signingCredentials: creds
      );

      return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string HashPassword(string password)
    {
      using var sha = SHA256.Create();
      var bytes = Encoding.UTF8.GetBytes(password);
      var hash = sha.ComputeHash(bytes);
      return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private bool VerifyPassword(string password, string storedHash)
    {
      var hashOfInput = HashPassword(password);
      return BCrypt.Net.BCrypt.Verify(password, storedHash);
    }

    public async Task<UserDto> DeleteUser(string email)
    {
      var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
      if (user == null)
        throw new Exception("User doesn't exist.");

      _context.Users.Remove(user);
      await _context.SaveChangesAsync();

      return new UserDto
      {
        Id = user.Id,
        UserName = user.UserName,
        Email = user.Email
      };
    }

    public async Task<string> ForgotPassword(ForgotPasswordDto forgot)
    {
      var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == forgot.Email);
      if (user == null) throw new Exception("User not found.");

      var token = Guid.NewGuid().ToString();


      user.ResetToken = token;
      user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);
      await _context.SaveChangesAsync();

      await _emailService.SendEmailResetare(user.Email, "Resetare parolă", $"Token resetare: {token}");

      return "Email cu instrucțiuni de resetare trimis.";
    }

    public async Task<string> ResetPassword(ResetPasswordDto reset)
    {
      var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == reset.Email);
      if (user == null) throw new Exception("User not found.");

      if (user.ResetToken != reset.Token || user.ResetTokenExpiry < DateTime.UtcNow)
        throw new Exception("Token invalid sau expirat.");

      user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(reset.Password);
      user.ResetToken = null;
      user.ResetTokenExpiry = null;

      await _context.SaveChangesAsync();

      return "Parola a fost resetată cu succes.";
    }
  }

}

