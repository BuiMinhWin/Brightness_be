using ChatService.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatService.Application.Interfaces
{
    public interface IConversationService
    {
        Task<SendMessageResponse> SendMessageAsync(Guid userId, SendMessageRequest request);
        Task<List<ChatMessageDTO>> GetSessionMessagesAsync(Guid sessionId);
        Task<List<ChatSessionDTO>> GetUserSessionsAsync(Guid userId);
        Task<ChatSessionDTO> CreateSessionAsync(Guid userId, string? title = null);
        Task CheckAndSummarizeIfNeededAsync(Guid sessionId);
    }
}

