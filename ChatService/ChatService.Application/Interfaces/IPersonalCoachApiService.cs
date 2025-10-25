using ChatService.Application.DTOs;

namespace ChatService.Application.Interfaces
{
    public interface IPersonalCoachApiService
    {
        // Schedule operations
        Task<object> CreateScheduleAsync(Guid userId, CreateScheduleFromChatRequest request);
        Task<List<object>> GetSchedulesAsync(Guid userId);
        Task<object?> GetScheduleByIdAsync(Guid scheduleId);
        Task<List<object>> GetUserSchedulesAsync(Guid userId);
        Task UpdateScheduleAsync(Guid scheduleId, Dictionary<string, object> updates);
        Task DeleteScheduleAsync(Guid scheduleId);
        
        // Profile operations
        Task<object?> GetUserProfileAsync(Guid userId);
        Task UpdateUserProfileAsync(Guid userId, Dictionary<string, object> updates);
        
        // Preference operations
        Task UpdateUserPreferenceAsync(Guid userId, PreferenceUpdateDTO preference);
        Task UpdateUserPreferencesAsync(Guid userId, Dictionary<string, object> updates);
        
        // Health data operations
        Task<List<object>> GetHealthDataHistoryAsync(Guid userId);
        Task DeleteHealthDataAsync(Guid userId, Guid healthDataId);
        
        // Progress operations
        Task<object?> GetProgressSummaryAsync(Guid userId);
        
        // Credit system integration
        Task<AICreditBalanceDTO?> GetCreditBalanceAsync(Guid userId);
        Task<bool> CanAffordChatMessageAsync(Guid userId);
        Task DeductChatMessageCreditAsync(Guid userId);
    }
}

