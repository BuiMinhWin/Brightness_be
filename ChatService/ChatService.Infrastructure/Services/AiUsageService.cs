using ChatService.Application.DTOs;
using ChatService.Application.Interfaces;
using ChatService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatService.Infrastructure.Services;

public class AiUsageService : IAiUsageService
{
    private readonly ChatDbContext _dbContext;

    public AiUsageService(ChatDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AiUsageSummaryDto> GetUsageSummaryAsync(CancellationToken cancellationToken = default)
    {
        var query = _dbContext.AiUsageLogs;

        // 1. Chạy query lấy tổng số
        var totals = await query
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Tokens = g.Sum(x => (long)x.TotalTokens),
                Cost = g.Sum(x => x.EstimatedCost)
            })
            .FirstOrDefaultAsync(cancellationToken);

        // 2. Chạy query lấy dữ liệu hàng ngày
        var daily = await query
            .GroupBy(log => log.CreatedAt.Date)
            .Select(g => new AiUsageDailyDto
            {
                Date = DateOnly.FromDateTime(g.Key),
                Tokens = g.Sum(x => (long)x.TotalTokens),
                Cost = g.Sum(x => x.EstimatedCost)
            })
            .OrderBy(x => x.Date)
            .ToListAsync(cancellationToken);

        // 3. Chạy query lấy ngày bắt đầu và kết thúc
        var dateRange = await query
            .GroupBy(_ => 1)
            .Select(g => new
            {
                MinDate = g.Min(x => x.CreatedAt),
                MaxDate = g.Max(x => x.CreatedAt)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return new AiUsageSummaryDto
        {
            StartDate = dateRange != null ? DateOnly.FromDateTime(dateRange.MinDate) : DateOnly.FromDateTime(DateTime.UtcNow),
            EndDate = dateRange != null ? DateOnly.FromDateTime(dateRange.MaxDate) : DateOnly.FromDateTime(DateTime.UtcNow),
            TotalTokens = totals?.Tokens ?? 0,
            TotalCost = totals?.Cost ?? 0,
            Daily = daily
        };
    }
}
