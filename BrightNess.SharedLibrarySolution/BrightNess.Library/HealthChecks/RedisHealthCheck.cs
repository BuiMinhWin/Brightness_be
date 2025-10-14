using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BrightNess.Library.HealthChecks
{
    public class RedisHealthCheck : IHealthCheck
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<RedisHealthCheck> _logger;

        public RedisHealthCheck(IConnectionMultiplexer redis, ILogger<RedisHealthCheck> logger)
        {
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var database = _redis.GetDatabase();
                var pingResult = await database.PingAsync();
                
                var endpoints = _redis.GetEndPoints();
                if (endpoints.Length > 0)
                {
                    var serverInfo = _redis.GetServer(endpoints[0]);
                    var info = await serverInfo.InfoAsync("memory");
                }
                
                var data = new Dictionary<string, object>
                {
                    { "ping_time_ms", pingResult.TotalMilliseconds },
                    { "connection_state", _redis.IsConnected ? "Connected" : "Disconnected" },
                    { "endpoints", string.Join(", ", _redis.GetEndPoints().Select(ep => ep.ToString())) }
                };

                _logger.LogDebug("Redis health check successful. Ping: {PingTime}ms", pingResult.TotalMilliseconds);

                return HealthCheckResult.Healthy("Redis is responsive", data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis health check failed");
                return HealthCheckResult.Unhealthy("Redis is not responsive", ex);
            }
        }
    }
}