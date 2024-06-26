﻿using BlameSightBackend.Models;
using BlameSightBackend.Models.Db;
using Microsoft.EntityFrameworkCore;

namespace BlameSightBackend.Services
{
    public class BlameService(BlameDbContext dbContext)
    {
        private readonly BlameDbContext _dbContext = dbContext;
        private readonly string _githubURL = "https://github.com/";

        public async Task<int> AddBlame(int blamerID, int blamedID, string path, int repoID, string message, int lineNumber, int urgency)
        {

            var newBlame = new Blame
            {
                BlameViewed = false,
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
        public async Task<List<viewBlame>> getMyCreatedBlames(int blamerID)
        {
            return await _dbContext.Blames
                                    .Include(b => b.Blamed)
                                    .Include(b => b.Repo)
                                    .ThenInclude(r => r.RepoOwner)
                                    .Include(b => b.UrgencyDescriptor)
                                    .Where(b => b.BlamerId == blamerID && !b.BlameComplete)
                                   .Select(b => new viewBlame
                                   {
                                       Id = b.BlameId,
                                       Name = b.Blamed.UserName,
                                       Path = $"{_githubURL}{b.Repo.RepoOwner.RepoOwnerName}/{b.Repo.RepoName}/blob/{b.BlamePath}#L{b.BlameLine}",
                                       LineNum = b.BlameLine,
                                       UrgencyDescriptor = b.UrgencyDescriptor.UrgencyDescriptorName,
                                       blameComplete = b.BlameComplete,
                                       blameViewed = b.BlameViewed,
                                       Comment = b.BlameMessage

                                   }

                )
                                   .ToListAsync();

        }
        public async Task<List<viewBlame>> getMyOpenBlames(int blamedD)
        {
            var blamelist = await _dbContext.Blames
                                   .Include(b => b.Blamer)
                                   .Include(b => b.Repo)
                                   .ThenInclude(r => r.RepoOwner)
                                   .Include(b => b.UrgencyDescriptor)
                                   .Where(b => b.BlamedId == blamedD && !b.BlameComplete)
                                   .ToListAsync();
            blamelist.ForEach(b => b.BlameViewed = true);
            await _dbContext.SaveChangesAsync();
            return blamelist
                                   .Select(b => new viewBlame
                                   {
                                       Id = b.BlameId,
                                       Name = b.Blamer.UserName,
                                       Path = $"{_githubURL}{b.Repo.RepoOwner.RepoOwnerName}/{b.Repo.RepoName}/blob/{b.BlamePath}#L{b.BlameLine}",
                                       LineNum = b.BlameLine,
                                       UrgencyDescriptor = b.UrgencyDescriptor.UrgencyDescriptorName,
                                       blameComplete = b.BlameComplete,
                                       blameViewed = b.BlameViewed,
                                       Comment = b.BlameMessage
                                   }

                )
                                   .ToList();

        }
        public async Task<bool?> setBlameComplete(int blamerID, int blameID)
        {
            var blame = _dbContext.Blames.Where(b => b.BlameId == blameID).FirstOrDefault();
            if (blame == null)
            {
                return null;
            }
            else if (blame.BlamerId != blamerID)return false;
            blame.BlameComplete = true;
            await _dbContext.SaveChangesAsync();
            return true;
        }
        public async Task<List<rankUser>> getBlameShame()
        {

            var topUsers = await _dbContext.Blames
            .Where(e => !e.BlameComplete)
            .GroupBy(e => e.Blamed)
            .Select(group => new rankUser
            {
                Name = group.Key.UserName,
                BlamePoints = group.Sum(e => e.UrgencyDescriptorId)
            })
            .OrderByDescending(result => result.BlamePoints)
            .Take(5)
            .ToListAsync();


            return topUsers;
        }
    }
}
