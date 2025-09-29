using System;

namespace NutritionAPI.Domain.Entities
{
    public class MealFood
    {
        public Guid Id { get; set; }
        public Guid MealLogId { get; set; }
        public string FoodName { get; set; } = null!;
        public string? ServingUnit { get; set; }
        public string? ServingSize { get; set; }
        public decimal? Calories { get; set; }
        public decimal? ProteinGrams { get; set; }
        public decimal? CarbGrams { get; set; }
        public decimal? FatGrams { get; set; }
    }
}
