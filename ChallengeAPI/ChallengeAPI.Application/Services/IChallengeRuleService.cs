
using ChallengeAPI.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChallengeAPI.Application.Services
{
    public interface IChallengeRuleService
    {
        Task<ChallengeRuleDTO> CreateChallengeRuleAsync(ChallengeRuleDTO challengeRuleDto);
        Task<ChallengeRuleDTO> GetChallengeRuleByIdAsync(Guid id);
        Task<IEnumerable<ChallengeRuleDTO>> GetAllChallengeRulesAsync();
        Task UpdateChallengeRuleAsync(Guid id, ChallengeRuleDTO challengeRuleDto);
        Task PatchChallengeRuleAsync(Guid id, Dictionary<string, object> patchData);
        Task DeleteChallengeRuleAsync(Guid id);
        Task<IEnumerable<ChallengeRuleDTO>> GetRulesByChallengeIdAsync(Guid challengeId);
    }
}
