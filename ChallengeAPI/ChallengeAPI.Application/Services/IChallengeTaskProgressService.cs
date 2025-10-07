
using ChallengeAPI.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChallengeAPI.Application.Services
{
    public interface IChallengeTaskProgressService
    {
        Task<ChallengeTaskProgressDTO> CreateChallengeTaskProgressAsync(ChallengeTaskProgressDTO challengeTaskProgressDto);
        Task<ChallengeTaskProgressDTO> GetChallengeTaskProgressByIdAsync(Guid id);
        Task<IEnumerable<ChallengeTaskProgressDTO>> GetAllChallengeTaskProgressesAsync();
        Task UpdateChallengeTaskProgressAsync(Guid id, ChallengeTaskProgressDTO challengeTaskProgressDto);
        Task PatchChallengeTaskProgressAsync(Guid id, Dictionary<string, object> patchData);
        Task DeleteChallengeTaskProgressAsync(Guid id);
        Task<IEnumerable<ChallengeTaskProgressDTO>> GetProgressByParticipantIdAsync(Guid participantId);
    }
}
