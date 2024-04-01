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
            // Extract the username claim from the JWT
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                // Assuming the claim type for the username is ClaimTypes.Name
                var usernameClaim = identity.FindFirst("Name")?.Value;
                if (usernameClaim != null)
                {
                    // Use the usernameClaim as needed for your logic
                    return Ok($"Hello, {usernameClaim}!");
                }
            }

            // If the claim is not found or the user is not authenticated
            return Unauthorized("User is not authenticated.");
        }
    }
}
