
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
    public class ChallengeParticipantRepository : IChallengeParticipantRepository
    {
        private readonly ChallengeDbContext _context;

        public ChallengeParticipantRepository(ChallengeDbContext context)
        {
            _context = context;
        }

        public async Task<ChallengeParticipant> AddAsync(ChallengeParticipant challengeParticipant)
        {
            _context.ChallengeParticipants.Add(challengeParticipant);
            await _context.SaveChangesAsync();
            return challengeParticipant;
        }

        public async Task<IEnumerable<ChallengeParticipant>> GetAllAsync()
        {
            return await _context.ChallengeParticipants.ToListAsync();
        }

        public async Task<ChallengeParticipant> GetByIdAsync(Guid id)
        {
            return await _context.ChallengeParticipants.FindAsync(id);
        }

        public async Task<IEnumerable<ChallengeParticipant>> GetByChallengeIdAsync(Guid challengeId)
        {
            return await _context.ChallengeParticipants.Where(p => p.ChallengeId == challengeId).ToListAsync();
        }

        public async Task UpdateAsync(ChallengeParticipant challengeParticipant)
        {
            _context.ChallengeParticipants.Update(challengeParticipant);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(ChallengeParticipant challengeParticipant)
        {
            _context.ChallengeParticipants.Remove(challengeParticipant);
            await _context.SaveChangesAsync();
        }
    }
}
