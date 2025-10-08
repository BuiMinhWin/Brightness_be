namespace AIOCRService.Application.Interfaces
{
    public interface IExternalAIService
    {
        Task<string> ProcessTextWithAIAsync(string extractedText, string prompt, string dataType);
        Task<string> GenerateSummaryAsync(string content);
        Task<string> GenerateInsightsAsync(string content, string dataType);
    }
}

