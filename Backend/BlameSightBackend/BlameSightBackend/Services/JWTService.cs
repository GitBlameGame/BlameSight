using System.Security.Claims;

namespace BlameSightBackend.Services
{
    public static class JWTService
    {

        public static string GetUsername(HttpContext httpContext)
        {
            // Extract the username claim from the JWT
            var identity = httpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                // Assuming the claim type for the username is ClaimTypes.Name
                var usernameClaim = identity.FindFirst("Name")?.Value;
                if (usernameClaim != null)
                {
                    // Use the usernameClaim as needed for your logic
                    return usernameClaim;
                }
                
            }
            return null;
        }
        public static string GetBearerToken(HttpContext httpContext)
        {
            var identity = httpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                // Assuming the claim type for the username is ClaimTypes.Name
                var tokenClaim = identity.FindFirst("Token")?.Value;
                if (tokenClaim != null)
                {
                    // Use the usernameClaim as needed for your logic
                    return tokenClaim;
                }

            }
            return null;
        }
    }
}
