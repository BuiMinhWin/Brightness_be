
using ChallengeAPI.Application.DTOs;
using ChallengeAPI.Application.Repositories;
using ChallengeAPI.Application.Services;
using ChallengeAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChallengeAPI.Application.Services
{
    public class AchievementService : IAchievementService
    {
        private readonly IAchievementRepository _achievementRepository;
        private readonly IChallengeParticipantRepository _challengeParticipantRepository;

        public AchievementService(IAchievementRepository achievementRepository, IChallengeParticipantRepository challengeParticipantRepository)
        {
            _achievementRepository = achievementRepository;
            _challengeParticipantRepository = challengeParticipantRepository;
        }

        public async Task<AchievementDTO> CreateAchievementAsync(AchievementDTO achievementDto)
        {
            if (!achievementDto.ChallengeId.HasValue)
            {
                throw new ArgumentException("ChallengeId is required.");
            }

            var participants = await _challengeParticipantRepository.GetByChallengeIdAsync(achievementDto.ChallengeId.Value);
            if (!participants.Any(p => p.UserId == achievementDto.UserId))
            {
                throw new ArgumentException("User is not a participant in this challenge.");
            }
            var achievement = new Achievement
            {
                UserId = achievementDto.UserId,
                ChallengeId = achievementDto.ChallengeId,
                Code = achievementDto.Code,
                Name = achievementDto.Name,
                Description = achievementDto.Description,
                CreatedAt = DateTime.UtcNow
            };

            var createdAchievement = await _achievementRepository.AddAsync(achievement);
            return new AchievementDTO(createdAchievement.Id, createdAchievement.UserId, createdAchievement.ChallengeId, createdAchievement.Code, createdAchievement.Name, createdAchievement.Description, createdAchievement.CreatedAt);
        }

        public async Task<AchievementDTO> GetAchievementByIdAsync(Guid id)
        {
            var achievement = await _achievementRepository.GetByIdAsync(id);
            if (achievement == null)
            {
                return null;
            }
            return new AchievementDTO(achievement.Id, achievement.UserId, achievement.ChallengeId, achievement.Code, achievement.Name, achievement.Description, achievement.CreatedAt);
        }

        public async Task<IEnumerable<AchievementDTO>> GetAllAchievementsAsync()
        {
            var achievements = await _achievementRepository.GetAllAsync();
            return achievements.Select(a => new AchievementDTO(a.Id, a.UserId, a.ChallengeId, a.Code, a.Name, a.Description, a.CreatedAt));
        }

        public async Task<IEnumerable<AchievementDTO>> GetAchievementsByUserIdAsync(Guid userId)
        {
            var achievements = await _achievementRepository.GetByUserIdAsync(userId);
            return achievements.Select(a => new AchievementDTO(a.Id, a.UserId, a.ChallengeId, a.Code, a.Name, a.Description, a.CreatedAt));
        }

        public async Task UpdateAchievementAsync(Guid id, AchievementDTO achievementDto)
        {
            var existingAchievement = await _achievementRepository.GetByIdAsync(id);
            if (existingAchievement == null)
            {
                throw new ArgumentException("Achievement not found.");
            }

            if (achievementDto.Name != null)
            {
                existingAchievement.Name = achievementDto.Name;
            }
            if (achievementDto.Description != null)
            {
                existingAchievement.Description = achievementDto.Description;
            }

            await _achievementRepository.UpdateAsync(existingAchievement);
        }

        public async Task DeleteAchievementAsync(Guid id)
        {
            var existingAchievement = await _achievementRepository.GetByIdAsync(id);
            if (existingAchievement == null)
            {
                throw new ArgumentException("Achievement not found.");
            }

            await _achievementRepository.DeleteAsync(existingAchievement);
        }
    }
}
