using BlameSightBackend.Models;
using BlameSightBackend.Services;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace BlameSightBackend.Controllers
{
    [Route("api/login")]
    [ApiController]
    public class LoginController(IConfiguration config, IHttpClientFactory httpClientFactory, UserService userService) : ControllerBase
    {
        private IConfiguration _config = config;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly UserService _userService=userService;


        [HttpGet]
        public IActionResult Login([FromHeader] string Authorization)
        {
            Console.WriteLine(Authorization);
            var user = GetUserNameFromGithub(Authorization).GetAwaiter().GetResult();
            //your logic for login process
            //If login usrename and password are correct then proceed to generate token

            if (user == null)
            {
                return BadRequest("Token invalid");
            }
            var claims = new List<Claim>
            {
                new Claim("Name",user),
                new Claim("Token", Authorization)
            };
/*            var userID= _userService.GetOrAddUserDB(user);
*/            var key = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? "";
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var Sectoken = new JwtSecurityToken(_config["JwtSettings:Issuer"],
              _config["JwtSettings:Issuer"],
              claims,
              expires: DateTime.Now.AddMinutes(120),
              signingCredentials: credentials);

            var token = new JwtSecurityTokenHandler().WriteToken(Sectoken);
/*            userID.Wait();
*/            return Ok(token);
        }

        public async Task<string> GetUserNameFromGithub(string token)
        {

            var httpClient = _httpClientFactory.CreateClient("GitHub");

            httpClient.DefaultRequestHeaders.Add("Authorization", $"bearer {token}");
            Console.WriteLine(httpClient.DefaultRequestHeaders);
            var httpResponse = await httpClient.GetAsync("user");
            Console.WriteLine(httpResponse);
            if (httpResponse.IsSuccessStatusCode)
            {
                using var contentStream = await httpResponse.Content.ReadAsStreamAsync();
                var jsonDocument = await JsonDocument.ParseAsync(contentStream);
                var login = jsonDocument.RootElement.GetProperty("login").GetString();
                return login;
            }
            else { return null; }

        }
       
    }
}
