using PersonalCoachAPI.Application.DTOs;
using System;
using System.Threading.Tasks;

namespace PersonalCoachAPI.Application.Interfaces
{
    public interface IManualScheduleService
    {
        Task<WorkoutScheduleResponse> CreateManualScheduleAsync(ManualScheduleRequest request);
        Task<WorkoutDayResponse> AddWorkoutDayFromTemplateAsync(AddDayFromTemplateRequest request);
        Task<WorkoutDayResponse> AddCustomWorkoutDayAsync(AddCustomDayRequest request);
        Task<WorkoutDayResponse> UpdateWorkoutDayAsync(Guid scheduleId, Guid dayId, UpdateWorkoutDayRequest request);
        Task DeleteWorkoutDayAsync(Guid scheduleId, Guid dayId);
    }
}




