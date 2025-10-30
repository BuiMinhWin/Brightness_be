namespace PersonalCoachAPI.Application.Interfaces;

public interface ICreditService
{
    /// <summary>
    /// Check if user can afford an operation based on estimated tokens
    /// </summary>
    Task<bool> CanAffordOperationAsync(string userId, string serviceName, int estimatedTokens);

    /// <summary>
    /// Deduct credits after an AI operation
    /// </summary>
    Task DeductCreditsAsync(
        string userId,
        string operationType,
        string modelUsed,
        int promptTokens,
        int completionTokens,
        string? referenceId = null);

    /// <summary>
    /// Get credit balance for a user
    /// </summary>
    Task<CreditBalanceDTO> GetCreditBalanceAsync(string userId, string serviceName);
}

public class CreditBalanceDTO
{
    public int RemainingTokens { get; set; }
    public int MonthlyTokens { get; set; }
    public int UsedTokens { get; set; }
    public DateTime ResetDate { get; set; }
    public string SubscriptionTier { get; set; } = string.Empty;
    public bool CanAfford { get; set; }
}

