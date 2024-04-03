using Microsoft.AspNetCore.Mvc;

namespace BlameSightBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthCheck : Controller
    {
        [HttpGet]
        public IActionResult checkHealth()
        {
            return Ok("Health GOOD");
        }
    }
}
