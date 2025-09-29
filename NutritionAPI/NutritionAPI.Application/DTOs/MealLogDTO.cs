using System;

namespace NutritionAPI.Application.DTOs
{
    public record MealLogDTO
    (
        Guid Id,
        Guid UserId,
        DateTime LoggedAt,
        string? MealType,
        string? Note,
        decimal? TotalCalories,
        decimal? ProteinGrams,
        decimal? CarbGrams,
        decimal? FatGrams
    );
}
