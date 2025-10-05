
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
    public class AchievementRepository : IAchievementRepository
    {
        private readonly ChallengeDbContext _context;

        public AchievementRepository(ChallengeDbContext context)
        {
            _context = context;
        }

        public async Task<Achievement> AddAsync(Achievement achievement)
        {
            _context.Achievements.Add(achievement);
            await _context.SaveChangesAsync();
            return achievement;
        }

        public async Task<IEnumerable<Achievement>> GetAllAsync()
        {
            return await _context.Achievements.ToListAsync();
        }

        public async Task<Achievement> GetByIdAsync(Guid id)
        {
            return await _context.Achievements.FindAsync(id);
        }

        public async Task<IEnumerable<Achievement>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Achievements.Where(a => a.UserId == userId).ToListAsync();
        }

        public async Task UpdateAsync(Achievement achievement)
        {
            _context.Achievements.Update(achievement);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Achievement achievement)
        {
            _context.Achievements.Remove(achievement);
            await _context.SaveChangesAsync();
        }
    }
}
