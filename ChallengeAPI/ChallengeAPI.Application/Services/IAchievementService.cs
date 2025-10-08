
using ChallengeAPI.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChallengeAPI.Application.Services
{
    public interface IAchievementService
    {
        Task<AchievementDTO> CreateAchievementAsync(AchievementDTO achievementDto);
        Task<AchievementDTO> GetAchievementByIdAsync(Guid id);
        Task<IEnumerable<AchievementDTO>> GetAllAchievementsAsync();
        Task UpdateAchievementAsync(Guid id, AchievementDTO achievementDto);
        Task DeleteAchievementAsync(Guid id);
        Task<IEnumerable<AchievementDTO>> GetAchievementsByUserIdAsync(Guid userId);
    }
}
