using ChatService.Application.Interfaces;
using ChatService.Domain.Entities;
using ChatService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatService.Infrastructure.Repositories
{
    public class ChatMessageRepository : IChatMessageRepository
    {
        private readonly ChatDbContext _context;

        public ChatMessageRepository(ChatDbContext context)
        {
            _context = context;
        }

        public async Task<ChatMessage> AddAsync(ChatMessage message)
        {
            message.CreatedAt = DateTime.UtcNow;
            await _context.ChatMessages.AddAsync(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<List<ChatMessage>> GetBySessionIdAsync(Guid sessionId)
        {
            return await _context.ChatMessages
                .Where(m => m.SessionId == sessionId)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetMessageCountBySessionIdAsync(Guid sessionId)
        {
            return await _context.ChatMessages
                .CountAsync(m => m.SessionId == sessionId);
        }

        public async Task<int> GetTodayMessageCountByUserIdAsync(Guid userId)
        {
            var today = DateTime.UtcNow.Date;
            return await _context.ChatMessages
                .Where(m => m.UserId == userId && m.Role == "user" && m.CreatedAt >= today)
                .CountAsync();
        }

        public async Task<int> GetMonthlyMessageCountByUserIdAsync(Guid userId)
        {
            var firstDayOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            return await _context.ChatMessages
                .Where(m => m.UserId == userId && m.Role == "user" && m.CreatedAt >= firstDayOfMonth)
                .CountAsync();
        }
    }
}

