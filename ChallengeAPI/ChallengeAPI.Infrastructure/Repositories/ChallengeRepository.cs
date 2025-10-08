
using ChallengeAPI.Application.Repositories;
using ChallengeAPI.Domain.Entities;
using ChallengeAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChallengeAPI.Infrastructure.Repositories
{
    public class ChallengeRepository : IChallengeRepository
    {
        private readonly ChallengeDbContext _context;

        public ChallengeRepository(ChallengeDbContext context)
        {
            _context = context;
        }

        public async Task<Challenge> AddAsync(Challenge challenge)
        {
            _context.Challenges.Add(challenge);
            await _context.SaveChangesAsync();
            return challenge;
        }

        public async Task<IEnumerable<Challenge>> GetAllAsync()
        {
            return await _context.Challenges.ToListAsync();
        }

        public async Task<Challenge> GetByIdAsync(Guid id)
        {
            return await _context.Challenges.FindAsync(id);
        }

        public async Task UpdateAsync(Challenge challenge)
        {
            _context.Challenges.Update(challenge);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Challenge challenge)
        {
            _context.Challenges.Remove(challenge);
            await _context.SaveChangesAsync();
        }

    }
}
