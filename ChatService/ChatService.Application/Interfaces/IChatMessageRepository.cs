using ChatService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatService.Application.Interfaces
{
    public interface IChatMessageRepository
    {
        Task<ChatMessage> AddAsync(ChatMessage message);
        Task<List<ChatMessage>> GetBySessionIdAsync(Guid sessionId);
        Task<int> GetMessageCountBySessionIdAsync(Guid sessionId);
        Task<int> GetTodayMessageCountByUserIdAsync(Guid userId);
        Task<int> GetMonthlyMessageCountByUserIdAsync(Guid userId);
    }
}

