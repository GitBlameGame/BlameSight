using BlameSightBackend.Models;
using BlameSightBackend.Models.Db;
using BlameSightBackend.Services;
using BlameSightBackend.Utils;
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
    public class BlameController(UserService userService, RepoService repoService, BlameService blameService) : Controller
    {
        private readonly UserService _userService = userService;
        private readonly RepoService _repoService = repoService;
        private readonly BlameService _blameService = blameService;
        [HttpPut]
        [Route("newBlame")]
        public async Task<IActionResult> newBlame([FromBody] newBlame blameInput)
        {
            var blamer = JWTUtils.GetUsername(HttpContext);
            var token = JWTUtils.GetBearerToken(HttpContext);
            if (blamer == null || token == null)
            {
                return Unauthorized("JWT Invalid, please login again");
            }
            if(blameInput.Comment.Length>256)
            { 
                return BadRequest("Blame Comment cannot exceed 256 characters. Please try to make your point concise.");
            }
            if (blameInput.Path.Count(f => f == '/') < 2)
            {
                return BadRequest("Path is not in the correct format\nowner/repo/pathtofile");
            }
            if (blameInput.Urgency < 1 || blameInput.Urgency > 5)
            {
                return BadRequest("Urgency level must be between 1 and 5.");
            }
            var client = new GraphQLClient(token);
            var query = client.generateBlameQL(blameInput);
            var response = await client.SendQueryAsync(query.ToString());

            var validate = ValidateResponse(response, blameInput);
            if (validate != null)
            {
                return validate;
            }
            var authorName = getBlamed(response, blameInput.LineNum);

            if (authorName == null)
            {
                return BadRequest("Line number is not valid");
            };

            var (repositoryOwner, repositoryName, filePath) = RepoUtils.ParsePath(blameInput.Path);

            var repoID =  _repoService.GetOrAddRepoDB(repositoryName, repositoryOwner);
            repoID.Wait();
            var blamedID = _userService.GetOrAddUserDB(authorName);
            blamedID.Wait();
            var blamerID = _userService.GetOrAddUserDB(blamer);
            
      
            blamerID.Wait();
            if (repoID == null || blamedID == null || blamerID == null)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    Error = "Error updating the DB\nPlease try again later"
                });

            };
            filePath = $"{blameInput.Branch}/{filePath}";
            var newBLame = _blameService.AddBlame(blamerID.Result, blamedID.Result, filePath, repoID.Result, blameInput.Comment, blameInput.LineNum, blameInput.Urgency);
            newBLame.Wait();
            if (newBLame == null)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    Error = "Error updating the DB\nPlease try again later"
                });

            };
            return Ok($"{authorName} was successfully blamed");
        }
        [HttpGet]
        [Route("myBlames")]
        public async Task<IActionResult> getMyCreatedBlames()
        {
            var blamerId = await _userService.GetOrAddUserDB(JWTUtils.GetUsername(HttpContext));
            var blamesOpen=await _blameService.getMyCreatedBlames(blamerId);
            if (blamesOpen.Any())
            {
                Console.WriteLine(blamesOpen.ToString());
                return Ok(blamesOpen);
              
                    
                    }

            return Ok("None of the blames you have created are currently open");
        }

        [HttpGet]
        [Route("openBlames")]
        public async Task<IActionResult> getOpenBlames()
        {
            var blamedId= await _userService.GetOrAddUserDB(JWTUtils.GetUsername(HttpContext));
            var blames= await _blameService.getMyOpenBlames(blamedId);
            if (blames.Any())
            {
                Console.WriteLine(blames.ToString());
                return Ok(blames);
            }

            return Ok("You have no pending blames\nCongrats🎊");
        }
        [HttpGet]
        [Route("blameBegone/{id}")]
        public async Task<IActionResult> setBlameComplete(int id)
        {
            var blamerId = await _userService.GetOrAddUserDB(JWTUtils.GetUsername(HttpContext));
            var setBlame=await _blameService.setBlameComplete(blamerId, id);
            switch (setBlame)
            {
                case null:
                    return NotFound("⚠️Blame with the given ID not found");
                case false:
                    return Unauthorized("🚨You don't have permission to close this blame");
                case true:
                    return Ok("Blame successfully closed😊");
            }

        }

        private string getBlamed(string response, int lineNum)
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
        private IActionResult ValidateResponse(string response, newBlame blameInput)
        {
            if (response.Contains("Bad credentials") || response.Contains("Token expired"))
            {
                return Unauthorized("Token is invalid, please login with Github again");
            }
            else if (response.Contains("\"ranges\":[]"))
            {
                return NotFound($"File not found on specified path: {blameInput.Path}");
            }
            else if (response.Contains("\"object\":null"))
            {
                return NotFound($"Branch {blameInput.Branch} cannot be resolved");
            }
            else if (response.Contains("NOT_FOUND"))
            {
                return NotFound($"Could not resolve a repository with the name: {blameInput.Path}\n" +
                    $"Please double-check the repository name for accuracy or verify that you have the necessary permissions to access it.");
            }
            return null; 
        }
    }
}
