
using ChallengeAPI.Application.Repositories;
using ChallengeAPI.Domain.Entities;
using ChallengeAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChallengeAPI.Infrastructure.Repositories
{
    public class ChallengeTaskProgressRepository : IChallengeTaskProgressRepository
    {
        private readonly ChallengeDbContext _context;

        public ChallengeTaskProgressRepository(ChallengeDbContext context)
        {
            _context = context;
        }

        public async Task<ChallengeTaskProgress> AddAsync(ChallengeTaskProgress challengeTaskProgress)
        {
            _context.ChallengeTaskProgresses.Add(challengeTaskProgress);
            await _context.SaveChangesAsync();
            return challengeTaskProgress;
        }

        public async Task<IEnumerable<ChallengeTaskProgress>> GetAllAsync()
        {
            return await _context.ChallengeTaskProgresses.ToListAsync();
        }

        public async Task<ChallengeTaskProgress> GetByIdAsync(Guid id)
        {
            return await _context.ChallengeTaskProgresses.FindAsync(id);
        }

        public async Task<IEnumerable<ChallengeTaskProgress>> GetByParticipantIdAsync(Guid participantId)
        {
            return await _context.ChallengeTaskProgresses.Where(p => p.ParticipantId == participantId).ToListAsync();
        }

        public async Task UpdateAsync(ChallengeTaskProgress challengeTaskProgress)
        {
            _context.ChallengeTaskProgresses.Update(challengeTaskProgress);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(ChallengeTaskProgress challengeTaskProgress)
        {
            _context.ChallengeTaskProgresses.Remove(challengeTaskProgress);
            await _context.SaveChangesAsync();
        }
    }
}
