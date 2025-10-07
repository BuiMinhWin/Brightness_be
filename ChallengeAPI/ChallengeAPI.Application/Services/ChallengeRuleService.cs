
using ChallengeAPI.Application.DTOs;
using ChallengeAPI.Application.Repositories;
using ChallengeAPI.Application.Services;
using ChallengeAPI.Domain.Entities;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChallengeAPI.Application.ServicesImpl
{
    public class ChallengeRuleService : IChallengeRuleService
    {
        private readonly IChallengeRuleRepository _challengeRuleRepository;
        private readonly IChallengeRepository _challengeRepository;

        public ChallengeRuleService(IChallengeRuleRepository challengeRuleRepository, IChallengeRepository challengeRepository)
        {
            _challengeRuleRepository = challengeRuleRepository;
            _challengeRepository = challengeRepository;
        }

        public async Task<ChallengeRuleDTO> CreateChallengeRuleAsync(ChallengeRuleDTO challengeRuleDto)
        {
            var challenge = await _challengeRepository.GetByIdAsync(challengeRuleDto.ChallengeId);
            if (challenge == null)
            {
                throw new ArgumentException("Challenge not found.");
            }

            var challengeRule = new ChallengeRule
            {
                ChallengeId = challengeRuleDto.ChallengeId,
                HowToParticipate = challengeRuleDto.HowToParticipate,
                RuleText = challengeRuleDto.RuleText
            };

            var createdChallengeRule = await _challengeRuleRepository.AddAsync(challengeRule);
            return new ChallengeRuleDTO(createdChallengeRule.Id, createdChallengeRule.ChallengeId, createdChallengeRule.HowToParticipate, createdChallengeRule.RuleText);
        }

        public async Task<ChallengeRuleDTO> GetChallengeRuleByIdAsync(Guid id)
        {
            var challengeRule = await _challengeRuleRepository.GetByIdAsync(id);
            if (challengeRule == null)
            {
                return null;
            }
            return new ChallengeRuleDTO(challengeRule.Id, challengeRule.ChallengeId, challengeRule.HowToParticipate, challengeRule.RuleText);
        }

        public async Task<IEnumerable<ChallengeRuleDTO>> GetAllChallengeRulesAsync()
        {
            var challengeRules = await _challengeRuleRepository.GetAllAsync();
            return challengeRules.Select(cr => new ChallengeRuleDTO(cr.Id, cr.ChallengeId, cr.HowToParticipate, cr.RuleText));
        }

        public async Task UpdateChallengeRuleAsync(Guid id, ChallengeRuleDTO challengeRuleDto)
        {
            var challenge = await _challengeRepository.GetByIdAsync(challengeRuleDto.ChallengeId);
            if (challenge == null)
            {
                throw new ArgumentException("Challenge not found.");
            }

            var existingChallengeRule = await _challengeRuleRepository.GetByIdAsync(id);
            if (existingChallengeRule == null)
            {
                throw new ArgumentException("Challenge rule not found.");
            }

            existingChallengeRule.ChallengeId = challengeRuleDto.ChallengeId;
            existingChallengeRule.HowToParticipate = challengeRuleDto.HowToParticipate;
            existingChallengeRule.RuleText = challengeRuleDto.RuleText;

            await _challengeRuleRepository.UpdateAsync(existingChallengeRule);
        }

        public async Task PatchChallengeRuleAsync(Guid id, Dictionary<string, object> patchData)
        {
            var existingChallengeRule = await _challengeRuleRepository.GetByIdAsync(id);
            if (existingChallengeRule == null)
            {
                throw new ArgumentException("Challenge rule not found.");
            }

            foreach (var kvp in patchData)
            {
                var propertyName = kvp.Key.ToLower();
                var value = kvp.Value;

                switch (propertyName)
                {
                    case "challengeid":
                        if (Guid.TryParse(value.ToString(), out var challengeId))
                        {
                            var challenge = await _challengeRepository.GetByIdAsync(challengeId);
                            if (challenge == null)
                            {
                                throw new ArgumentException("Challenge not found.");
                            }
                            existingChallengeRule.ChallengeId = challengeId;
                        }
                        break;
                    case "howtoparticipate":
                        if (value is JsonElement jsonElementHow)
                        {
                            existingChallengeRule.HowToParticipate = JsonSerializer.Deserialize<List<string>>(jsonElementHow.GetRawText());
                        }
                        break;
                    case "ruletext":
                        if (value is JsonElement jsonElementRule)
                        {
                            existingChallengeRule.RuleText = JsonSerializer.Deserialize<List<string>>(jsonElementRule.GetRawText());
                        }
                        break;
                }
            }

            await _challengeRuleRepository.UpdateAsync(existingChallengeRule);
        }

        public async Task DeleteChallengeRuleAsync(Guid id)
        {
            var existingChallengeRule = await _challengeRuleRepository.GetByIdAsync(id);
            if (existingChallengeRule == null)
            {
                throw new ArgumentException("Challenge rule not found.");
            }

            await _challengeRuleRepository.DeleteAsync(existingChallengeRule);
        }

        public async Task<IEnumerable<ChallengeRuleDTO>> GetRulesByChallengeIdAsync(Guid challengeId)
        {
            var challengeRules = await _challengeRuleRepository.GetByChallengeIdAsync(challengeId);
            return challengeRules.Select(cr => new ChallengeRuleDTO(cr.Id, cr.ChallengeId, cr.HowToParticipate, cr.RuleText));
        }
    }
}
