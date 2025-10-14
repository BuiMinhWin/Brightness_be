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
    public class ChatSessionRepository : IChatSessionRepository
    {
        private readonly ChatDbContext _context;

        public ChatSessionRepository(ChatDbContext context)
        {
            _context = context;
        }

        public async Task<ChatSession> AddAsync(ChatSession session)
        {
            session.CreatedAt = DateTime.UtcNow;
            await _context.ChatSessions.AddAsync(session);
            await _context.SaveChangesAsync();
            return session;
        }

        public async Task<ChatSession?> GetByIdAsync(Guid id)
        {
            return await _context.ChatSessions.FindAsync(id);
        }

        public async Task<List<ChatSession>> GetByUserIdAsync(Guid userId)
        {
            return await _context.ChatSessions
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task UpdateAsync(ChatSession session)
        {
            session.UpdatedAt = DateTime.UtcNow;
            _context.ChatSessions.Update(session);
            await _context.SaveChangesAsync();
        }
    }
}

