using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace BrightNess.Library.RateLimiting
{
    public class RedisRateLimitService : IRateLimitService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<RedisRateLimitService> _logger;
        private readonly string _keyPrefix = "rate_limit:";

        public RedisRateLimitService(IConnectionMultiplexer redis, ILogger<RedisRateLimitService> logger)
        {
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<RateLimitResult> CheckRateLimitAsync(string key, int limit, TimeSpan window)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));

            var fullKey = $"{_keyPrefix}{key}";
            var windowSeconds = (int)window.TotalSeconds;

            try
            {
                var db = _redis.GetDatabase();
                
                // Lua script for atomic rate limiting check
                var script = @"
                    local key = KEYS[1]
                    local window = tonumber(ARGV[1])
                    local limit = tonumber(ARGV[2])
                    local now = tonumber(ARGV[3])
                    
                    local current = redis.call('GET', key)
                    local windowStart = now - (now % window)
                    local windowEnd = windowStart + window
                    
                    if current == false then
                        redis.call('SET', key, 1)
                        redis.call('EXPIREAT', key, windowEnd)
                        return {1, limit - 1, windowEnd - now}
                    else
                        local count = tonumber(current)
                        if count < limit then
                            local newCount = redis.call('INCR', key)
                            return {1, limit - newCount, windowEnd - now}
                        else
                            return {0, 0, windowEnd - now}
                        end
                    end";

                var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var result = await db.ScriptEvaluateAsync(script, 
                    new RedisKey[] { fullKey }, 
                    new RedisValue[] { windowSeconds, limit, now });

                var resultArray = (RedisValue[])result!;
                var isAllowed = resultArray[0] == 1;
                var remaining = (int)resultArray[1];
                var retryAfterSeconds = (int)resultArray[2];

                return new RateLimitResult
                {
                    IsAllowed = isAllowed,
                    RemainingRequests = Math.Max(0, remaining),
                    RetryAfter = TimeSpan.FromSeconds(retryAfterSeconds),
                    WindowResetTime = DateTime.UtcNow.AddSeconds(retryAfterSeconds)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Rate limit check failed for key {Key}", key);
                
                // In case of error, allow the request to prevent blocking legitimate traffic
                return new RateLimitResult
                {
                    IsAllowed = true,
                    RemainingRequests = limit,
                    RetryAfter = TimeSpan.Zero,
                    WindowResetTime = DateTime.UtcNow.Add(window)
                };
            }
        }

        public async Task<RateLimitResult> CheckUserRateLimitAsync(string userId, string action, int limit, TimeSpan window)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
            
            if (string.IsNullOrWhiteSpace(action))
                throw new ArgumentException("Action cannot be null or empty", nameof(action));

            var key = $"user:{userId}:{action}";
            return await CheckRateLimitAsync(key, limit, window);
        }
    }
}