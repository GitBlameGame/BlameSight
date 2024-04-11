using BlameSightBackend.Models.Db;
using Microsoft.EntityFrameworkCore;

namespace BlameSightBackend.Services
{
    public class RepoService(BlameDbContext dbContext)
    {
        private readonly BlameDbContext _dbContext = dbContext;

        public async Task<int> GetOrAddRepoDB(string RepoName, string RepoOwner)
        {
            var repoOwnerId = await GetOrAddRepOwnerDB(RepoOwner);
            var Repo = await _dbContext.Repos.FirstOrDefaultAsync(u => u.RepoName == RepoName && u.RepoOwnerId == repoOwnerId);
            if (Repo == null)
            {
                var newRepo = new Repo
                {
                    RepoName = RepoName,
                    RepoOwnerId = repoOwnerId,
                };
                _dbContext.Repos.Add(newRepo);
                await _dbContext.SaveChangesAsync();
                return newRepo.RepoId;
            }
            return Repo.RepoId;
        }
        public async Task<int> GetOrAddRepOwnerDB(string RepoOwner)
        {
            var RepoOwners = await _dbContext.RepoOwners.FirstOrDefaultAsync(u => u.RepoOwnerName == RepoOwner);
            if (RepoOwners == null)
            {
                var newRepoOwners = new Repoowner
                {
                    RepoOwnerName = RepoOwner,
                };
                _dbContext.RepoOwners.Add(newRepoOwners);
                await _dbContext.SaveChangesAsync();
                return newRepoOwners.RepoOwnerId;
            }
            return RepoOwners.RepoOwnerId;
        }

    }
}
