using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SessionController : ControllerBase
    {
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            HttpContext.Session.SetString("LastActivity", DateTime.UtcNow.ToString("o"));
            return Ok(new { message = "pong" });
        }
    }
}
