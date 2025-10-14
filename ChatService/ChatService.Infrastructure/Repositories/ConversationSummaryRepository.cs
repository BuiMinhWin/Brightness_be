using ChatService.Application.Interfaces;
using ChatService.Domain.Entities;
using ChatService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ChatService.Infrastructure.Repositories
{
    public class ConversationSummaryRepository : IConversationSummaryRepository
    {
        private readonly ChatDbContext _context;

        public ConversationSummaryRepository(ChatDbContext context)
        {
            _context = context;
        }

        public async Task<ConversationSummary> AddAsync(ConversationSummary summary)
        {
            summary.CreatedAt = DateTime.UtcNow;
            await _context.ConversationSummaries.AddAsync(summary);
            await _context.SaveChangesAsync();
            return summary;
        }

        public async Task<ConversationSummary?> GetLatestBySessionIdAsync(Guid sessionId)
        {
            return await _context.ConversationSummaries
                .Where(s => s.SessionId == sessionId)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();
        }
    }
}

