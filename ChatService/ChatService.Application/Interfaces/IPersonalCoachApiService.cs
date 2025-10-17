using ChatService.Application.DTOs;

namespace ChatService.Application.Interfaces
{
    public interface IPersonalCoachApiService
    {
        Task<object> CreateScheduleAsync(Guid userId, CreateScheduleFromChatRequest request);
        Task<List<object>> GetUserSchedulesAsync(Guid userId);
        Task UpdateUserPreferenceAsync(Guid userId, PreferenceUpdateDTO preference);
    }
}

