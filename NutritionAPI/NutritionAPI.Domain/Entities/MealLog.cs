using System;
using System.Collections.Generic;

namespace NutritionAPI.Domain.Entities
{
    public class MealLog
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime LoggedAt { get; set; }
        public string? MealType { get; set; }
        public string? Note { get; set; }
        public decimal? TotalCalories { get; set; }
        public decimal? ProteinGrams { get; set; }
        public decimal? CarbGrams { get; set; }
        public decimal? FatGrams { get; set; }
        public ICollection<MealFood> Foods { get; set; } = new List<MealFood>();
    }
}
