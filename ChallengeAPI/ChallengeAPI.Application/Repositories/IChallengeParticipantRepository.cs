
using ChallengeAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChallengeAPI.Application.Repositories
{
    public interface IChallengeParticipantRepository
    {
        Task<ChallengeParticipant> AddAsync(ChallengeParticipant challengeParticipant);
        Task<ChallengeParticipant> GetByIdAsync(Guid id);
        Task<IEnumerable<ChallengeParticipant>> GetAllAsync();
        Task UpdateAsync(ChallengeParticipant challengeParticipant);
        Task DeleteAsync(ChallengeParticipant challengeParticipant);
        Task<IEnumerable<ChallengeParticipant>> GetByChallengeIdAsync(Guid challengeId);
    }
}
