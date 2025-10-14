using System;
using System.Threading.Tasks;
using BrightNess.Library.Security;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace BrightNess.Library.Security
{
    public class RedisJwtBlacklistService : IJwtBlacklistService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<RedisJwtBlacklistService> _logger;
        private readonly string _keyPrefix = "jwt:blacklist:";

        public RedisJwtBlacklistService(IConnectionMultiplexer redis, ILogger<RedisJwtBlacklistService> logger)
        {
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task BlacklistTokenAsync(string jti, DateTime expiry)
        {
            if (string.IsNullOrWhiteSpace(jti))
            {
                _logger.LogWarning("Attempted to blacklist token with empty JTI");
                return;
            }

            try
            {
                var db = _redis.GetDatabase();
                var key = $"{_keyPrefix}{jti}";
                var ttl = expiry - DateTime.UtcNow;

                if (ttl > TimeSpan.Zero)
                {
                    await db.StringSetAsync(key, "1", ttl);
                    _logger.LogInformation("Token {JTI} blacklisted until {Expiry}", jti, expiry);
                }
                else
                {
                    _logger.LogWarning("Token {JTI} is already expired, not blacklisting", jti);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to blacklist token {JTI}", jti);
                throw;
            }
        }

        public async Task<bool> IsTokenBlacklistedAsync(string jti)
        {
            if (string.IsNullOrWhiteSpace(jti))
            {
                return false;
            }

            try
            {
                var db = _redis.GetDatabase();
                var key = $"{_keyPrefix}{jti}";
                return await db.KeyExistsAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check if token {JTI} is blacklisted", jti);
                // In case of Redis failure, assume token is not blacklisted to avoid blocking valid requests
                return false;
            }
        }

        public async Task BlacklistUserTokensAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("Attempted to blacklist tokens for empty user ID");
                return;
            }

            try
            {
                var db = _redis.GetDatabase();
                var userBlacklistKey = $"user:blacklist:{userId}";
                
                // Set a flag indicating all tokens for this user should be considered blacklisted
                // This is more efficient than blacklisting individual tokens
                await db.StringSetAsync(userBlacklistKey, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), TimeSpan.FromDays(30));
                
                _logger.LogInformation("All tokens for user {UserId} have been blacklisted", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to blacklist tokens for user {UserId}", userId);
                throw;
            }
        }
    }
}