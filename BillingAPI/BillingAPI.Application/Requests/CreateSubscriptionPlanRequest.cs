using System;
using System.ComponentModel.DataAnnotations;

namespace BillingAPI.Application.Requests
{
    public class CreateSubscriptionPlanRequest
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = null!;

        [MaxLength(1024)]
        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        [MaxLength(10)]
        public string Currency { get; set; } = "VND";

        [Range(1, 3650)]
        public int DurationDays { get; set; } = 30;

        [Range(0, 100000)]
        public int MonthlyAICredits { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }
}
