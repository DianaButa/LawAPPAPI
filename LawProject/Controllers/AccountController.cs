using LawProject.DTO;
using LawProject.Service.AccountService;
using LawProject.Service.FileService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LawProject.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class AccountController : ControllerBase
  {

    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
      this._accountService = accountService;
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet("delete/{email}")]
    public async Task<UserDto> DeleteUser(string email)
    {
      return await _accountService.DeleteUser(email);
    }


    [HttpGet("Users")]
    public async Task<IActionResult> GetAllUsers()
    {
      try
      {
        var allusers = await _accountService.GetAllFilesAsync();
        return Ok(allusers);
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"Internal server error: {ex.Message}");
      }
    }


    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<AuthenticationResultDto> Login([FromBody] UserDto user)
    {
      return await _accountService.Login(user);
    }

    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register([FromBody] UserDto user)
    {
      return Ok(await _accountService.Register(user));
    }

    [HttpPost("forgotPassword")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
    {
      return Ok(await _accountService.ForgotPassword(model));
    }

    [HttpPost("resetPassword")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPassword)
    {
      return Ok(await _accountService.ResetPassword(resetPassword));
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
      try
      {
        var result = await _accountService.ChangePassword(dto);
        return Ok(new { message = result });
      }
      catch (Exception ex)
      {
        return BadRequest(new { error = ex.Message });
      }
    }


  }
}
