using System;
using System.Threading.Tasks;

namespace BrightNess.Library.RateLimiting
{
    public interface IRateLimitService
    {
        Task<RateLimitResult> CheckRateLimitAsync(string key, int limit, TimeSpan window);
        Task<RateLimitResult> CheckUserRateLimitAsync(string userId, string action, int limit, TimeSpan window);
    }

    public class RateLimitResult
    {
        public bool IsAllowed { get; set; }
        public int RemainingRequests { get; set; }
        public TimeSpan RetryAfter { get; set; }
        public DateTime WindowResetTime { get; set; }
    }
}