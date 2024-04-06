using BlameSightBackend.Models.Db;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlameSightBackend.Controllers
{
    [ApiController]
    [Route("api/Users")]
    public class UserController(BlameDbContext dbContext) : Controller
    {
        private readonly BlameDbContext _dbContext=dbContext;
        [HttpPut]
        public async Task< IActionResult> addUser([FromBody] string userName )
        {
            // Check if the username already exists
            if (await _dbContext.Users.AnyAsync(u => u.UserName == userName))
            {
                return BadRequest("Username already exists. Choose a different username.");
            }
            var newUser = new User
            {
                UserName = userName,
            };
            _dbContext.Users.Add(newUser);
            _dbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> getAllUsers()
        {
            return Ok(_dbContext.Users.ToList());
        }
    }
}
