using ChatService.Application.DTOs;

namespace ChatService.Application.Interfaces
{
    public interface IPersonalCoachApiService
    {
        Task<object> CreateScheduleAsync(Guid userId, CreateScheduleFromChatRequest request);
        Task<List<object>> GetUserSchedulesAsync(Guid userId);
        Task UpdateUserPreferenceAsync(Guid userId, PreferenceUpdateDTO preference);
        
        // Credit system integration
        Task<AICreditBalanceDTO?> GetCreditBalanceAsync(Guid userId);
        Task<bool> CanAffordChatMessageAsync(Guid userId);
        Task DeductChatMessageCreditAsync(Guid userId);
    }
}

