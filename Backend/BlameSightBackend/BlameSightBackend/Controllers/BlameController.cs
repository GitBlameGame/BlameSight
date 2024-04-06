﻿using BlameSightBackend.Models;
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
    public class BlameController(UserService userService, RepoService repoService, BlameService blameService) : Controller
    {
        private readonly UserService _userService = userService;
        private readonly RepoService _repoService = repoService;
        private readonly BlameService _blameService = blameService;
        [HttpPut]
        public async Task<IActionResult> newBlame([FromBody] newBlame blameInput)
        {
            var blamer = JWTService.GetUsername(HttpContext);
            var token = JWTService.GetBearerToken(HttpContext);
            if (blamer == null || token == null)
            {
                return Unauthorized("JWT Invalid, please login again");
            }

            if (blameInput.Path.Count(f => f == '/') < 2)
            {
                return BadRequest("Path is not in the correct format\nowner/repo/pathtofile");
            }
            if (blameInput.Urgency < 1 || blameInput.Urgency > 5)
            {
                return BadRequest("Urgency must be 1 to 5");
            }
            var client = new GraphQLClient(token);
            var query = client.generateBlameQL(blameInput);
            var response = await client.SendQueryAsync(query.ToString());

            var validate = ValidateResponse(response, blameInput);
            if(validate is not OkObjectResult)
            {
                return validate;
            }
            var authorName = getBlamed(response, blameInput.LineNum);

            if (authorName == null)
            {
                return BadRequest("Line number is not valid");
            };

            var (repositoryOwner, repositoryName, filePath) = Utils.ParsePath(blameInput.Path);

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
        public IActionResult ValidateResponse(string response, newBlame blameInput)
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

            // If none of the conditions are met, you can return a default response or continue processing
            return Ok(); // Or another appropriate response
        }
    }
}
