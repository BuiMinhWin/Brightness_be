using System;
using System.Collections.Generic;

namespace PersonalCoachAPI.Application.DTOs
{
    public record ExerciseLibraryDTO(
        Guid Id,
        string ExerciseName,
        string ExerciseNameVi,
        string Category,
        List<string> TargetMuscles,
        string Difficulty,
        List<string> EquipmentRequired,
        string Description,
        string VideoUrl,
        int DefaultSets,
        string DefaultReps,
        int DefaultRestSeconds
    );

    public record ExerciseFilters(
        string? Category,
        string? Difficulty,
        List<string>? EquipmentAvailable,
        string? SearchQuery
    );

    public record ExerciseSuggestionRequest(
        Guid UserId,
        string FocusArea,
        string Difficulty,
        List<string>? EquipmentAvailable,
        int Count = 5
    );
}




