using PersonalCoachAPI.Application.DTOs;
using System;
using System.Threading.Tasks;

namespace PersonalCoachAPI.Application.Interfaces
{
    public interface IAIScheduleService
    {
        // Generate new schedule (FR-007)
        Task<WorkoutScheduleResponse> GenerateScheduleAsync(GenerateScheduleRequest request);
        
        // Modify specific day without regenerating full schedule
        Task<ModifiedDayResponse> ModifyScheduleDayAsync(ModifyDayRequest request);
        
        // Adjust difficulty based on feedback (FR-009)
        Task<ScheduleAdjustmentResponse> AdjustScheduleDifficultyAsync(AdjustDifficultyRequest request);
        
        // Get schedule details
        Task<WorkoutScheduleResponse> GetScheduleAsync(Guid scheduleId);
    }
}

