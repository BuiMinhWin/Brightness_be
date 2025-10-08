
using ChallengeAPI.Application.Repositories;
using ChallengeAPI.Domain.Entities;
using ChallengeAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChallengeAPI.Infrastructure.Repositories
{
    public class ChallengeTaskRepository : IChallengeTaskRepository
    {
        private readonly ChallengeDbContext _context;

        public ChallengeTaskRepository(ChallengeDbContext context)
        {
            _context = context;
        }

        public async Task<ChallengeTask> AddAsync(ChallengeTask challengeTask)
        {
            _context.ChallengeTasks.Add(challengeTask);
            await _context.SaveChangesAsync();
            return challengeTask;
        }

        public async Task<IEnumerable<ChallengeTask>> GetAllAsync()
        {
            return await _context.ChallengeTasks.ToListAsync();
        }

        public async Task<ChallengeTask> GetByIdAsync(Guid id)
        {
            return await _context.ChallengeTasks.FindAsync(id);
        }

        public async Task UpdateAsync(ChallengeTask challengeTask)
        {
            _context.ChallengeTasks.Update(challengeTask);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(ChallengeTask challengeTask)
        {
            _context.ChallengeTasks.Remove(challengeTask);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ChallengeTask>> GetByChallengeIdAsync(Guid challengeId)
        {
            return await _context.ChallengeTasks.Where(t => t.ChallengeId == challengeId).ToListAsync();
        }
    }
}
