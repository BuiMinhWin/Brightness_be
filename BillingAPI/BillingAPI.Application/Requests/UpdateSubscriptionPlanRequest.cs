using System;
using System.ComponentModel.DataAnnotations;

namespace BillingAPI.Application.Requests
{
    public class UpdateSubscriptionPlanRequest
    {
        [MaxLength(200)]
        public string? Name { get; set; }

        [MaxLength(1024)]
        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? Price { get; set; }

        [MaxLength(10)]
        public string? Currency { get; set; }

        [Range(1, 3650)]
        public int? DurationDays { get; set; }

        [Range(0, 100000)]
        public int? MonthlyAICredits { get; set; }

        public bool? IsActive { get; set; }
    }
}
