using System;
using System.Collections.Generic;

namespace PersonalCoachAPI.Application.DTOs
{
    public record WorkoutTemplateDTO(
        Guid Id,
        string TemplateName,
        string FocusArea,
        string Difficulty,
        int EstimatedDuration,
        string Description,
        List<TemplateExerciseDTO> Exercises
    );

    public record TemplateExerciseDTO(
        Guid ExerciseLibraryId,
        string ExerciseName,
        string ExerciseNameVi,
        int OrderIndex,
        int Sets,
        string Reps,
        int RestSeconds,
        string Notes
    );

    public record TemplateFilters(
        string? FocusArea,
        string? Difficulty
    );
}




