using ChatService.Application.DTOs;

namespace ChatService.Application.Interfaces
{
    public interface IIntentDetectionService
    {
        Task<IntentDetectionResult> DetectIntentAsync(string userMessage);
    }
}

