
using ChallengeAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChallengeAPI.Application.Repositories
{
    public interface IChallengeRuleRepository
    {
        Task<ChallengeRule> AddAsync(ChallengeRule challengeRule);
        Task<ChallengeRule> GetByIdAsync(Guid id);
        Task<IEnumerable<ChallengeRule>> GetAllAsync();
        Task UpdateAsync(ChallengeRule challengeRule);
        Task UpdatePatchAsync(ChallengeRule challengeRule);
        Task DeleteAsync(ChallengeRule challengeRule);
        Task<IEnumerable<ChallengeRule>> GetByChallengeIdAsync(Guid challengeId);
    }
}
