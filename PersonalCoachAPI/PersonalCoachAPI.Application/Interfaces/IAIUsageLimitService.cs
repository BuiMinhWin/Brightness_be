using PersonalCoachAPI.Application.DTOs;
using System;
using System.Threading.Tasks;

namespace PersonalCoachAPI.Application.Interfaces
{
    public interface IAIUsageLimitService
    {
        Task<AIUsageLimitsDTO> GetUsageLimitsAsync(Guid userId);
        Task<bool> CanGenerateAsync(Guid userId);
        Task TrackGenerationAsync(Guid userId, string generationType, Guid? relatedScheduleId = null);
    }
}




