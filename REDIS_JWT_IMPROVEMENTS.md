# ?? Redis & JWT Improvements for Brightness System

## 1. ?? JWT Security Enhancements

### Current Issues:
- Symmetric key s? d?ng cho t?t c? services
- ClockSkew không consistent gi?a các service
- Không có JWT blacklist mechanism

### Improvements:

#### A. Implement JWT Blacklist v?i Redis
```csharp
public interface IJwtBlacklistService
{
    Task BlacklistTokenAsync(string jti, DateTime expiry);
    Task<bool> IsTokenBlacklistedAsync(string jti);
}

public class RedisJwtBlacklistService : IJwtBlacklistService
{
    private readonly IConnectionMultiplexer _redis;
    
    public async Task BlacklistTokenAsync(string jti, DateTime expiry)
    {
        var db = _redis.GetDatabase();
        var ttl = expiry - DateTime.UtcNow;
        if (ttl > TimeSpan.Zero)
        {
            await db.StringSetAsync($"jwt:blacklist:{jti}", "1", ttl);
        }
    }
    
    public async Task<bool> IsTokenBlacklistedAsync(string jti)
    {
        var db = _redis.GetDatabase();
        return await db.KeyExistsAsync($"jwt:blacklist:{jti}");
    }
}
```

#### B. Enhanced JWT Configuration
```csharp
public class JwtOptions
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
    public int RefreshTokenExpirationDays { get; set; } = 14;
    public int ClockSkewMinutes { get; set; } = 2; // Consistent across services
    public bool RequireHttpsMetadata { get; set; } = true; // Production should be true
    public bool ValidateLifetime { get; set; } = true;
    public string Algorithm { get; set; } = SecurityAlgorithms.HmacSha256;
}
```

## 2. ?? Redis Optimization

### Current Issues:
- Không có Redis health check
- Không có connection pooling configuration
- Không có retry mechanism cho Redis operations

### Improvements:

#### A. Redis Configuration Enhancement
```csharp
public class RedisOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public int Database { get; set; } = 0;
    public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(5);
    public TimeSpan SyncTimeout { get; set; } = TimeSpan.FromSeconds(5);
    public int ConnectRetry { get; set; } = 3;
    public bool AbortOnConnectFail { get; set; } = false;
    public string KeyPrefix { get; set; } = "brightness:";
}
```

#### B. Redis Service v?i Retry Pattern
```csharp
public class ResilientRedisService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<ResilientRedisService> _logger;
    private readonly RetryPolicy _retryPolicy;

    public async Task<T?> GetAsync<T>(string key)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var db = _redis.GetDatabase();
            var value = await db.StringGetAsync(key);
            return value.IsNull ? default : JsonSerializer.Deserialize<T>(value!);
        });
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            var db = _redis.GetDatabase();
            var serialized = JsonSerializer.Serialize(value);
            await db.StringSetAsync(key, serialized, expiry);
        });
    }
}
```

## 3. ??? Security Improvements

### A. Rate Limiting v?i Redis
```csharp
public class RedisRateLimiter
{
    private readonly IConnectionMultiplexer _redis;
    
    public async Task<bool> IsAllowedAsync(string key, int limit, TimeSpan window)
    {
        var db = _redis.GetDatabase();
        var script = @"
            local key = KEYS[1]
            local window = tonumber(ARGV[1])
            local limit = tonumber(ARGV[2])
            local current = redis.call('GET', key)
            
            if current == false then
                redis.call('SET', key, 1)
                redis.call('EXPIRE', key, window)
                return 1
            else
                if tonumber(current) < limit then
                    return redis.call('INCR', key)
                else
                    return -1
                end
            end";
            
        var result = await db.ScriptEvaluateAsync(script, new RedisKey[] { key }, new RedisValue[] { window.TotalSeconds, limit });
        return result != -1;
    }
}
```

### B. Session Management v?i Redis
```csharp
public class RedisSessionService
{
    public async Task CreateSessionAsync(string userId, string sessionId, TimeSpan duration)
    {
        var db = _redis.GetDatabase();
        var sessionKey = $"session:{userId}:{sessionId}";
        var userSessionsKey = $"user_sessions:{userId}";
        
        // Store session data
        await db.HashSetAsync(sessionKey, new HashEntry[]
        {
            new("user_id", userId),
            new("created_at", DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
            new("last_accessed", DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        });
        
        await db.KeyExpireAsync(sessionKey, duration);
        await db.SetAddAsync(userSessionsKey, sessionId);
        await db.KeyExpireAsync(userSessionsKey, duration);
    }
}
```

## 4. ?? Monitoring & Health Checks

### A. Redis Health Check
```csharp
public class RedisHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer _redis;
    
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            await db.PingAsync();
            return HealthCheckResult.Healthy("Redis is responsive");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Redis is not responsive", ex);
        }
    }
}
```

### B. JWT Health Check
```csharp
public class JwtHealthCheck : IHealthCheck
{
    private readonly ITokenService _tokenService;
    
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var testPayload = new TokenPayload { UserId = Guid.NewGuid(), Email = "test@test.com" };
            var token = _tokenService.GenerateAccessToken(testPayload);
            
            if (string.IsNullOrEmpty(token))
                return HealthCheckResult.Unhealthy("JWT token generation failed");
                
            return HealthCheckResult.Healthy("JWT service is working");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("JWT service failed", ex);
        }
    }
}
```

## 5. ?? Configuration Improvements

### A. Environment-specific Redis Settings
```json
// appsettings.Production.json
{
  "Redis": {
    "ConnectionString": "your-production-redis-connection",
    "Database": 0,
    "KeyPrefix": "brightness:prod:",
    "ConnectTimeout": "00:00:10",
    "SyncTimeout": "00:00:10",
    "ConnectRetry": 5,
    "AbortOnConnectFail": false
  },
  "Authentication": {
    "Key": "your-production-key-minimum-32-characters",
    "RequireHttpsMetadata": true,
    "ClockSkewMinutes": 1
  }
}
```

### B. Distributed Cache cho t?t c? Services
```csharp
// In each service's Program.cs
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = configuration.GetConnectionString("Redis");
    options.InstanceName = "Brightness";
});
```

## 6. ?? Performance Optimizations

### A. Redis Pipeline for Batch Operations
```csharp
public async Task InvalidateMultipleTokensAsync(IEnumerable<string> tokens)
{
    var db = _redis.GetDatabase();
    var batch = db.CreateBatch();
    
    var tasks = tokens.Select(token => 
        batch.KeyDeleteAsync($"auth:refresh:{token}")).ToArray();
    
    batch.Execute();
    await Task.WhenAll(tasks);
}
```

### B. JWT Claims Optimization
```csharp
public static class OptimizedClaims
{
    // Use shorter claim names to reduce token size
    public const string UserId = "uid";
    public const string Email = "eml";
    public const string Role = "rol";
    public const string Permissions = "prm";
}
```

## ?? Implementation Priority

1. **High Priority**:
   - JWT Blacklist implementation
   - Redis health checks
   - Rate limiting
   - Environment-specific configurations

2. **Medium Priority**:
   - Resilient Redis service
   - Session management
   - Performance optimizations

3. **Low Priority**:
   - Advanced monitoring
   - Pipeline optimizations
   - Claims optimization

## ?? Next Steps

1. Implement JWT blacklist service
2. Add Redis health checks to all services
3. Configure rate limiting for authentication endpoints
4. Set up proper production configurations
5. Add monitoring and alerting for Redis connections