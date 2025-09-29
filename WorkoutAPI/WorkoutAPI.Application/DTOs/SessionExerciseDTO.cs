using System;

namespace WorkoutAPI.Application.DTOs
{
    public record SessionExerciseDTO
    (
        Guid Id,
        Guid WorkoutSessionId,
        string ExerciseName,
        int OrderNo,
        int? Sets,
        int? Reps,
        decimal? WeightKg,
        int? DistanceMeters,
        int? DurationSeconds,
        int? RestSeconds
    );
}
