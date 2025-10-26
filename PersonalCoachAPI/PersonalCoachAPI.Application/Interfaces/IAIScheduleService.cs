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
        
        // Get all schedules for a user
        Task<List<WorkoutScheduleResponse>> GetUserSchedulesAsync(Guid userId);
        
        // Mark exercise as completed
        Task MarkExerciseCompleteAsync(Guid exerciseId, bool completed, int? actualSets, int? actualReps);
        
        // New Full Suite Features
        
        // Submit workout feedback
        Task SubmitWorkoutFeedbackAsync(SubmitWorkoutFeedbackRequest request);
        
        // Regenerate schedule from specific day forward
        Task<WorkoutScheduleResponse> RegenerateScheduleFromDayAsync(RegenerateScheduleRequest request);
        
        // Edit individual workout day
        Task<ScheduleDayDetail> EditWorkoutDayAsync(EditWorkoutDayRequest request);
        
        // Get schedule history and versions
        Task<ScheduleHistoryResponse> GetScheduleHistoryAsync(Guid userId);
        
        // Get analytics for user's workout patterns
        Task<ScheduleAnalytics> GetScheduleAnalyticsAsync(Guid userId);
    }
}

