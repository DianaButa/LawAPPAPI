using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LawProject.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class HealthController : ControllerBase
  {
    [HttpGet]
    [Route("/health")]
    public IActionResult Get()
    {
      return Ok("Healthy");
    }
  }
}
