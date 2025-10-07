
using ChallengeAPI.Application.Repositories;
using ChallengeAPI.Domain.Entities;
using ChallengeAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChallengeAPI.Infrastructure.Repositories
{
    public class ChallengeRuleRepository : IChallengeRuleRepository
    {
        private readonly ChallengeDbContext _context;

        public ChallengeRuleRepository(ChallengeDbContext context)
        {
            _context = context;
        }

        public async Task<ChallengeRule> AddAsync(ChallengeRule challengeRule)
        {
            _context.ChallengeRules.Add(challengeRule);
            await _context.SaveChangesAsync();
            return challengeRule;
        }

        public async Task<IEnumerable<ChallengeRule>> GetAllAsync()
        {
            return await _context.ChallengeRules.ToListAsync();
        }

        public async Task<ChallengeRule> GetByIdAsync(Guid id)
        {
            return await _context.ChallengeRules.FindAsync(id);
        }

        public async Task UpdateAsync(ChallengeRule challengeRule)
        {
            _context.ChallengeRules.Update(challengeRule);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePatchAsync(ChallengeRule entity)
        {
            var existing = await _context.ChallengeRules.FindAsync(entity.Id);
            if (existing == null)
                throw new ArgumentException("ChallengeRule not found");

            // Cập nhật từng field có thay đổi
            if (entity.HowToParticipate != null)
                existing.HowToParticipate = entity.HowToParticipate;

            if (entity.RuleText != null)
                existing.RuleText = entity.RuleText;

            if (entity.ChallengeId != Guid.Empty)
                existing.ChallengeId = entity.ChallengeId;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(ChallengeRule challengeRule)
        {
            _context.ChallengeRules.Remove(challengeRule);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ChallengeRule>> GetByChallengeIdAsync(Guid challengeId)
        {
            return await _context.ChallengeRules.Where(r => r.ChallengeId == challengeId).ToListAsync();
        }
    }
}
