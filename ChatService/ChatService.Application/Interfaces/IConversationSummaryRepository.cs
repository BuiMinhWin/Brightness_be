using ChatService.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace ChatService.Application.Interfaces
{
    public interface IConversationSummaryRepository
    {
        Task<ConversationSummary> AddAsync(ConversationSummary summary);
        Task<ConversationSummary?> GetLatestBySessionIdAsync(Guid sessionId);
    }
}

