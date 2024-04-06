using BlameSightBackend.Models;
using BlameSightBackend.Models.Db;
using BlameSightBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace BlameSightBackend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/Blames")]
    public class BlameController : Controller
    {
        [HttpPut]
        public async Task<IActionResult> newBlame([FromBody] newBlame blameInput)
        {
            var blamer = JWTService.GetUsername(HttpContext);
            var token= JWTService.GetBearerToken(HttpContext);
            if (blamer==null || token==null )
            {
                return Unauthorized("JWT Invalid, please login again");
            }

            if (blameInput.Path.Count(f => f == '/') < 2)
            {
                return BadRequest("Path is not in the correct format\nowner/repo/pathtofile");
            }
       
            var client = new GraphQLClient(token);
            var query = client.generateBlameQL(blameInput);
            var response = await client.SendQueryAsync(query.ToString());
            if (response.Contains("NOT_FOUND"))
            {
                return NotFound($"Could not resolve a repository with the name: {blameInput.Path}");
            }
            var authorName = getBlamed(response, blameInput.LineNum);

            if (authorName == null)
            {
                return BadRequest("Line number is not valid");
            };
            return Ok(authorName);
        }

        public string getBlamed(string response, int lineNum)
        {
            using JsonDocument doc = JsonDocument.Parse(response);
            JsonElement root = doc.RootElement;

            JsonElement extract = root.GetProperty("data").GetProperty("repository").GetProperty("object").GetProperty("blame").GetProperty("ranges");

            var authorName = extract.EnumerateArray()
                .Where(extract => extract.GetProperty("startingLine").GetInt32() <= lineNum && extract.GetProperty("endingLine").GetInt32() >= lineNum)
                .Select(extract => extract.GetProperty("commit").GetProperty("author").GetProperty("name").GetString())
                .FirstOrDefault();
            return authorName;
        }
    }
}
