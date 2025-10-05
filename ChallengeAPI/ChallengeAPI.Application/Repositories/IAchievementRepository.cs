
using ChallengeAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChallengeAPI.Application.Repositories
{
    public interface IAchievementRepository
    {
        Task<Achievement> AddAsync(Achievement achievement);
        Task<Achievement> GetByIdAsync(Guid id);
        Task<IEnumerable<Achievement>> GetAllAsync();
        Task UpdateAsync(Achievement achievement);
        Task DeleteAsync(Achievement achievement);
        Task<IEnumerable<Achievement>> GetByUserIdAsync(Guid userId);
    }
}
