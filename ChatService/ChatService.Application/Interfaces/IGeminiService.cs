using ChatService.Application.DTOs;
using System.Threading.Tasks;

namespace ChatService.Application.Interfaces
{
    public interface IGeminiService
    {
        Task<string> GenerateResponseAsync(string userMessage, UserContextDTO userContext);
        Task<string> SummarizeConversationAsync(string conversationHistory);
    }
}

