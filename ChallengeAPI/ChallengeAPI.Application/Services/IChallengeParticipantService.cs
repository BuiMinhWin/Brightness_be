
using ChallengeAPI.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChallengeAPI.Application.Services
{
    public interface IChallengeParticipantService
    {
        Task<ChallengeParticipantDTO> CreateChallengeParticipantAsync(ChallengeParticipantDTO challengeParticipantDto);
        Task<ChallengeParticipantDTO> GetChallengeParticipantByIdAsync(Guid id);
        Task<IEnumerable<ChallengeParticipantDTO>> GetAllChallengeParticipantsAsync();
        Task UpdateChallengeParticipantAsync(Guid id, ChallengeParticipantDTO challengeParticipantDto);
        Task DeleteChallengeParticipantAsync(Guid id);
        Task<IEnumerable<ChallengeParticipantDTO>> GetParticipantsByChallengeIdAsync(Guid challengeId);
    }
}
