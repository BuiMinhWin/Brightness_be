using System;

namespace NutritionAPI.Application.DTOs
{
    public record MealFoodDTO
    (
        Guid Id,
        Guid MealLogId,
        string FoodName,
        string? ServingUnit,
        string? ServingSize,
        decimal? Calories,
        decimal? ProteinGrams,
        decimal? CarbGrams,
        decimal? FatGrams
    );
}
