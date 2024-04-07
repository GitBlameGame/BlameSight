﻿using BlameSightBackend.Models;
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
        public async Task<List<viewBlame>> getMyOpenBlames(int blamerID)
        {
                return await _dbContext.Blames.Include(b => b.Blamed).Include(b => b.Repo).ThenInclude(r => r.RepoOwner).Include(b => b.UrgencyDescriptor)
                                       .Where(b => b.BlamerId == blamerID && !b.BlameComplete)
                                       .Select(b => new viewBlame
                                       {
                                           Id=b.BlameId,
                                           Name=b.Blamed.UserName,
                                           Path=$"{b.Repo.RepoOwner.RepoOwnerName}/{b.Repo.RepoName}/{b.BlamePath}",
                                           LineNum=b.BlameLine,
                                           UrgencyDescriptor=b.UrgencyDescriptor.UrgencyDescriptorName,
                                           blameComplete=b.BlameComplete,
                                           blameViewed=b.BlameAccepted,
                                           Comment=b.BlameMessage
                                          
                                       }

                    )
                                       .ToListAsync();
           
        }
        
    }
}
