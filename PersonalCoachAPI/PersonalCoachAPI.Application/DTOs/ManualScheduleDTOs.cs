using System;
using System.Collections.Generic;

namespace PersonalCoachAPI.Application.DTOs
{
    public record ManualScheduleRequest(
        Guid UserId,
        string ScheduleName,
        string Goal,
        DateTime StartDate,
        DateTime? EndDate,
        int DurationDays,
        string DifficultyLevel
    );

    public record AddDayFromTemplateRequest(
        Guid ScheduleId,
        Guid TemplateId,
        DateTime Date,
        int DayNumber
    );

    public record AddCustomDayRequest(
        Guid ScheduleId,
        DateTime Date,
        int DayNumber,
        string DayType,
        string FocusArea,
        int EstimatedDuration,
        List<CustomExerciseDTO> Exercises
    );

    public record CustomExerciseDTO(
        Guid? ExerciseLibraryId,
        string ExerciseName,
        string Category,
        int Sets,
        string Reps,
        int RestSeconds,
        int OrderIndex,
        string? Notes
    );

    public record UpdateWorkoutDayRequest(
        string? DayType,
        string? FocusArea,
        int? EstimatedDuration,
        List<CustomExerciseDTO>? Exercises
    );

    public record WorkoutDayResponse(
        Guid DayId,
        Guid ScheduleId,
        int DayNumber,
        DateTime Date,
        string DayType,
        string FocusArea,
        int EstimatedDuration,
        List<ExerciseDetailDTO> Exercises
    );

    public record ExerciseDetailDTO(
        Guid ExerciseId,
        string ExerciseName,
        string Category,
        int Sets,
        string Reps,
        int RestSeconds,
        int OrderIndex,
        string Notes
    );
}




