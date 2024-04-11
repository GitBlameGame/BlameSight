using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlameSightBackend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/hello")]
    public class HelloWorldController : Controller
    {
        [HttpGet]
        public IActionResult GetHelloWorld()
        {
        
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var usernameClaim = identity.FindFirst("Name")?.Value;
                if (usernameClaim != null)
                {
                    return Ok($"Hello, {usernameClaim}!");
                }
            }

            return Unauthorized("User is not authenticated.");
        }
    }
}
