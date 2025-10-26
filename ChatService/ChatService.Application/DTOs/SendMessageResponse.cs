using System;

namespace ChatService.Application.DTOs
{
    public record SendMessageResponse
    (
        Guid SessionId,
        string Message,
        string Role,
        int RemainingMessages,  // Legacy - for backward compatibility
        int DailyLimit,         // Legacy - for backward compatibility
        DateTime CreatedAt,
        // New credit-based fields
        int? RemainingCredits = null,
        int? MonthlyCredits = null,
        string? SubscriptionTier = null
    );
}

