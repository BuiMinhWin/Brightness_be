using ChatService.Application.DTOs;

namespace ChatService.Application.Interfaces
{
    public interface IProfileAnalysisService
    {
        Task<ScheduleRecommendationDTO> AnalyzeUserProfileAsync(Guid userId);
    }

    public record ScheduleRecommendationDTO(
        string Goal,
        string DifficultyLevel,
        int DurationDays,
        string Reasoning
    );
}

