using ChatService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatService.Application.Interfaces
{
    public interface IChatSessionRepository
    {
        Task<ChatSession> AddAsync(ChatSession session);
        Task<ChatSession?> GetByIdAsync(Guid id);
        Task<List<ChatSession>> GetByUserIdAsync(Guid userId);
        Task UpdateAsync(ChatSession session);
    }
}

