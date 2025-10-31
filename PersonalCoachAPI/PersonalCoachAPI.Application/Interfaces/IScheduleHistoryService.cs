using PersonalCoachAPI.Application.DTOs;

namespace PersonalCoachAPI.Application.Interfaces
{
    public interface IScheduleHistoryService
    {
        Task RecordScheduleActionAsync(
            Guid scheduleId,
            Guid userId,
            string actionType,
            int version,
            string? changeReason = null,
            string? userNotes = null,
            int? fromDayNumber = null,
            string? previousDifficulty = null,
            string? newDifficulty = null,
            int tokensUsed = 0);

        Task<List<ScheduleHistoryEntry>> GetUserScheduleHistoryAsync(Guid userId, int limit = 50);

        Task<List<ScheduleHistoryEntry>> GetScheduleVersionHistoryAsync(Guid scheduleId);

        Task<UserScheduleAnalytics> GetScheduleAnalyticsAsync(Guid userId);

        Task<WorkoutScheduleResponse> RevertToVersionAsync(Guid scheduleId, Guid userId);
    }
}
