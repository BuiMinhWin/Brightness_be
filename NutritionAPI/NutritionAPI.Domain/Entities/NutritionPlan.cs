using System;
using System.Collections.Generic;

namespace NutritionAPI.Domain.Entities
{
    public class NutritionPlan
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; } = null!;
        public decimal? CalorieTarget { get; set; }
        public decimal? ProteinGrams { get; set; }
        public decimal? CarbGrams { get; set; }
        public decimal? FatGrams { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string Status { get; set; } = "draft";
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public ICollection<MealLog> MealLogs { get; set; } = new List<MealLog>();
    }
}
