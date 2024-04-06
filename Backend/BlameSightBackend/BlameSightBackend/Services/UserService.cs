using BlameSightBackend.Models.Db;
using Microsoft.EntityFrameworkCore;

namespace BlameSightBackend.Services
{
    public class UserService(BlameDbContext dbContext)
    {
        private readonly BlameDbContext _dbContext = dbContext;

        public async Task<int> GetOrAddUserDB(string userName)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == userName);
            if (user == null)
            {
                var newUser = new User
                {
                    UserName = userName,
                };
                _dbContext.Users.Add(newUser);
                await _dbContext.SaveChangesAsync();
                return newUser.UserId;
            }
            return user.UserId;
        }

    }
}
