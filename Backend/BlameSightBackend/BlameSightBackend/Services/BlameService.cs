using BlameSightBackend.Models.Db;
using Microsoft.EntityFrameworkCore;

namespace BlameSightBackend.Services
{
    public class BlameService(BlameDbContext dbContext)
    {
        private readonly BlameDbContext _dbContext = dbContext;

        public async Task<int> AddBlame(int blamerID, int blamedID, string path, int repoID, string message, int lineNumber, int urgency)
        {
           
                var newBlame = new Blame
                {
                   BlameAccepted = false,
                   BlameComplete = false,
                   BlamerId = blamerID,
                   BlamedId = blamedID,
                   BlamePath = path,
                   RepoId = repoID,
                   BlameMessage = message,
                   BlameLine = lineNumber,
                   UrgencyDescriptorId = urgency

                };
                _dbContext.Blames.Add(newBlame);
                await _dbContext.SaveChangesAsync();
                return newBlame.BlameId;
          
        }
        
    }
}
