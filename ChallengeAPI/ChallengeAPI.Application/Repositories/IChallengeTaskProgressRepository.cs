
using ChallengeAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChallengeAPI.Application.Repositories
{
    public interface IChallengeTaskProgressRepository
    {
        Task<ChallengeTaskProgress> AddAsync(ChallengeTaskProgress challengeTaskProgress);
        Task<ChallengeTaskProgress> GetByIdAsync(Guid id);
        Task<IEnumerable<ChallengeTaskProgress>> GetAllAsync();
        Task UpdateAsync(ChallengeTaskProgress challengeTaskProgress);
        Task DeleteAsync(ChallengeTaskProgress challengeTaskProgress);
        Task<IEnumerable<ChallengeTaskProgress>> GetByParticipantIdAsync(Guid participantId);
    }
}
