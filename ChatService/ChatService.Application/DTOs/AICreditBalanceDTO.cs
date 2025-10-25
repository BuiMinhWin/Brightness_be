using System;
using System.Collections.Generic;

namespace ChatService.Application.DTOs
{
    // Mirrors PersonalCoachAPI.Application.DTOs.AICreditBalanceDTO
    public class AICreditBalanceDTO
    {
        public int RemainingCredits { get; set; }
        public int MonthlyCredits { get; set; }
        public bool CanAffordOperation { get; set; }
        public int UsedCreditsThisMonth { get; set; }
        public int DaysUntilReset { get; set; }
        public string SubscriptionTier { get; set; } = string.Empty;
        public Dictionary<string, int>? CreditCostBreakdown { get; set; }
    }
}

