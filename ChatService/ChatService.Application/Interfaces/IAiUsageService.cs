using ChatService.Application.DTOs;

namespace ChatService.Application.Interfaces;

public interface IAiUsageService
{
    Task<AiUsageSummaryDto> GetUsageSummaryAsync(CancellationToken cancellationToken = default);
}
