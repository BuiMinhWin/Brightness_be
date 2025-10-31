namespace AIOCRService.Application.Interfaces;

public interface ICreditService
{
    /// <summary>
    /// Deduct credits after an AI OCR operation
    /// </summary>
    Task DeductCreditsAsync(
        Guid userId,
        string operationType,
        string modelUsed,
        int promptTokens,
        int completionTokens,
        string? referenceId = null);
}

