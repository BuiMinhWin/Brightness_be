using System;
using System.Text.Json;
using System.Threading.Tasks;
using AuthenticationAPI.Application.Interfaces;
using AuthenticationAPI.Domain.Entities;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace AuthenticationAPI.Infrastructure.Caching
{
    public class RefreshTokenCacheService : IRefreshTokenCacheService
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly ILogger<RefreshTokenCacheService> _logger;
        private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

        public RefreshTokenCacheService(IConnectionMultiplexer connectionMultiplexer, ILogger<RefreshTokenCacheService> logger)
        {
            _connectionMultiplexer = connectionMultiplexer ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task CacheTokenAsync(RefreshToken token)
        {
            ArgumentNullException.ThrowIfNull(token);
            if (string.IsNullOrWhiteSpace(token.Token))
            {
                throw new ArgumentException("Refresh token value cannot be empty.", nameof(token));
            }

            var db = _connectionMultiplexer.GetDatabase();
            var ttl = token.ExpiresAt - DateTime.UtcNow;
            if (ttl <= TimeSpan.Zero)
            {
                ttl = TimeSpan.FromSeconds(30);
            }

            var cacheValue = JsonSerializer.Serialize(new CachedRefreshToken
            {
                UserId = token.UserId,
                ExpiresAt = token.ExpiresAt
            }, _serializerOptions);

            await db.StringSetAsync(BuildTokenKey(token.Token), cacheValue, ttl).ConfigureAwait(false);
            var userKey = BuildUserKey(token.UserId);
            await db.SetAddAsync(userKey, token.Token).ConfigureAwait(false);
            await db.KeyExpireAsync(userKey, token.ExpiresAt).ConfigureAwait(false);
        }

        public async Task RemoveTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return;
            }

            var db = _connectionMultiplexer.GetDatabase();
            var tokenKey = BuildTokenKey(token);
            var payload = await db.StringGetAsync(tokenKey).ConfigureAwait(false);
            if (!payload.IsNullOrEmpty)
            {
                try
                {
                    var cached = JsonSerializer.Deserialize<CachedRefreshToken>(payload!, _serializerOptions);
                    if (cached is not null && cached.UserId != Guid.Empty)
                    {
                        await db.SetRemoveAsync(BuildUserKey(cached.UserId), token).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to deserialize cached refresh token for cleanup.");
                }
            }

            await db.KeyDeleteAsync(tokenKey).ConfigureAwait(false);
        }

        public async Task RemoveTokensByUserAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return;
            }

            var db = _connectionMultiplexer.GetDatabase();
            var userKey = BuildUserKey(userId);
            var tokenEntries = await db.SetMembersAsync(userKey).ConfigureAwait(false);
            foreach (var entry in tokenEntries)
            {
                if (!entry.IsNullOrEmpty)
                {
                    await db.KeyDeleteAsync(BuildTokenKey(entry!)).ConfigureAwait(false);
                }
            }

            await db.KeyDeleteAsync(userKey).ConfigureAwait(false);
        }

        private static RedisKey BuildTokenKey(string token) => new($"auth:refresh:{token}");

        private static RedisKey BuildUserKey(Guid userId) => new($"auth:user:{userId}:tokens");

        private sealed class CachedRefreshToken
        {
            public Guid UserId { get; set; }
            public DateTime ExpiresAt { get; set; }
        }
    }
}
