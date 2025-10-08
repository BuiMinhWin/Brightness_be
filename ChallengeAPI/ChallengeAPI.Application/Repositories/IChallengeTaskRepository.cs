
using ChallengeAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChallengeAPI.Application.Repositories
{
    public interface IChallengeTaskRepository
    {
        Task<ChallengeTask> AddAsync(ChallengeTask challengeTask);
        Task<ChallengeTask> GetByIdAsync(Guid id);
        Task<IEnumerable<ChallengeTask>> GetAllAsync();
        Task UpdateAsync(ChallengeTask challengeTask);
        Task DeleteAsync(ChallengeTask challengeTask);
        Task<IEnumerable<ChallengeTask>> GetByChallengeIdAsync(Guid challengeId);
    }
}
