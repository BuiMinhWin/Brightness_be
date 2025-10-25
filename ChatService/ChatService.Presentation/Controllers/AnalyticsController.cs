using ChatService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.Presentation.Controllers;

[Authorize]
[ApiController]
[Route("api/Chat")]
public class AnalyticsController(IAiUsageService aiUsageService) : ControllerBase
{
    private readonly IAiUsageService _aiUsageService = aiUsageService;

    [HttpGet("usage")]
    public async Task<IActionResult> GetUsageAsync(CancellationToken cancellationToken = default)
    {
        var summary = await _aiUsageService.GetUsageSummaryAsync(cancellationToken);
        return Ok(summary);
    }
}
