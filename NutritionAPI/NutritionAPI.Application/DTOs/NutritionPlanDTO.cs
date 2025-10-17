using System;

namespace NutritionAPI.Application.DTOs
{
    public record NutritionPlanDTO
    (
        Guid Id,
        Guid UserId,
        string Name,
        decimal? CalorieTarget,
        decimal? ProteinGrams,
        decimal? CarbGrams,
        decimal? FatGrams,
        DateOnly? StartDate,
        DateOnly? EndDate,
        string Status,
        DateTime CreatedAt,
        DateTime? UpdatedAt
    );
}
