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
            var segments = blameInput.Path.TrimStart('/').Split('/');
                // Extract the repository owner, repository name, and path to file
                var repositoryOwner = segments.Length > 0 ? segments[0] : string.Empty;
                var repositoryName = segments.Length > 1 ? segments[1] : string.Empty;
                var filePath = segments.Length > 2 ? string.Join("/", segments.Skip(2)) : string.Empty;




            /*  Attributes ghattr = new();
              ghattr.add("owner", "Luke-Bradford489");
              ghattr.add("name", "SudokuSolver-Genetic_Algorithm");

              GraphObject ghobject = new([new GraphField("name"), new GraphField("url")]);
              GraphField ghfield = new("repository", attributes: ghattr, graphObject: ghobject);

              GraphQuery query2 = new();*/
            Attributes ghattr = new();
            ghattr.add("owner", repositoryOwner);
            ghattr.add("name", repositoryName);

            // Add the branch and file path as attributes
            ghattr.add("path", $"main:{filePath}");

            // Define the blame range fields
            GraphObject blameRangeObject = new([new GraphField("startingLine"), new GraphField("endingLine"), new GraphField("commit" ,graphObject:
                new GraphObject([new GraphField("author", graphObject: new([new GraphField("name")]))])) ] );

            GraphField ghfield = new("repository", attributes: ghattr, graphObject: blameRangeObject);
            GraphQuery query = new();
            query.add(ghfield);
            Console.WriteLine(query.ToString());
            var client = new GraphQLClient(token);
            var response = await client.SendQueryAsync(query.ToString());


            // Define the blame field
            /*GraphField blameField = new("blame", attributes: new Attributes().add("path", "path/to/file"), graphObject: blameRangeObject);

            // Add the blame field to the repository object

            GraphField ghfield = new("repository", attributes: ghattr, graphObject: ghobject);

            GraphQuery query2 = new();
            query2.add(ghfield);

            Console.WriteLine("\n\n-----------------------------");

            Console.WriteLine("Actual query: " + query2);


            var client = new GraphQLClient( token);
            // Uses the query taken directly from a GraphQL explorer payload
            // Uses a query made using code
            var response2 = await client.SendQueryAsync(query2.ToString());
            Console.WriteLine("\n\n-----------------------------");

            Console.WriteLine(response2);*/
            return Ok(response);
        }
    }
}
