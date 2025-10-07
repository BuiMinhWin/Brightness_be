
using ChallengeAPI.Application.DTOs;
using ChallengeAPI.Domain.Entities;
using Microsoft.AspNetCore.JsonPatch;

namespace ChallengeAPI.Application.Services
{
    public interface IChallengeService
    {

        Task<Challenge> CreateChallengeAsync(ChallengeDTO challengeDto);
        Task UpdateChallengeAsync(Guid id, ChallengeDTO challengeDto);
   
        Task PatchChallengeAsync(Guid id, Dictionary<string, object> patchData);
        Task DeleteChallengeAsync(Guid id);
        Task<ChallengeDTO> GetChallengeByIdAsync(Guid id);
        Task<IEnumerable<ChallengeDTO>> GetAllChallengesAsync();
        Task<IEnumerable<ChallengeRuleDTO>> GetChallengeRulesAsync(Guid challengeId);
        Task<IEnumerable<ChallengeTaskDTO>> GetChallengeTasksAsync(Guid challengeId);
        Task<IEnumerable<ChallengeParticipantDTO>> GetChallengeParticipantsAsync(Guid challengeId);

        Task<ChallengeTaskProgressDTO> GetTaskProgressAsync(Guid taskId);
        Task UpdateTaskProgressAsync(Guid taskId, ChallengeTaskProgressDTO progressDto);

     
    }
}
