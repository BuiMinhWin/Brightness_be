using ChatService.Application.DTOs;
using ChatService.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ChatService.Infrastructure.Services
{
    public class UserContextCacheService : IUserContextCacheService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<UserContextCacheService> _logger;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

        public UserContextCacheService(IMemoryCache cache, ILogger<UserContextCacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public Task<UserContextDTO?> GetCachedContextAsync(Guid sessionId)
        {
            var cacheKey = GetCacheKey(sessionId);
            
            if (_cache.TryGetValue(cacheKey, out UserContextDTO? cachedContext))
            {
                _logger.LogDebug("Cache hit for session {SessionId}", sessionId);
                return Task.FromResult(cachedContext);
            }

            _logger.LogDebug("Cache miss for session {SessionId}", sessionId);
            return Task.FromResult<UserContextDTO?>(null);
        }

        public Task SetCachedContextAsync(Guid sessionId, UserContextDTO context)
        {
            var cacheKey = GetCacheKey(sessionId);
            
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheDuration,
                SlidingExpiration = TimeSpan.FromMinutes(15)
            };

            _cache.Set(cacheKey, context, cacheOptions);
            _logger.LogDebug("Cached context for session {SessionId} (TTL: {Duration})", sessionId, CacheDuration);

            return Task.CompletedTask;
        }

        public Task InvalidateCacheAsync(Guid sessionId)
        {
            var cacheKey = GetCacheKey(sessionId);
            _cache.Remove(cacheKey);
            _logger.LogInformation("Invalidated cache for session {SessionId}", sessionId);

            return Task.CompletedTask;
        }

        private static string GetCacheKey(Guid sessionId) => $"user_context_{sessionId}";
    }
}

