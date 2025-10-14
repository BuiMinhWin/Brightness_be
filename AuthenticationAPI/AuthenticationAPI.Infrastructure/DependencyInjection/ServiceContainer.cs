using AuthenticationAPI.Application.Interfaces;
using AuthenticationAPI.Application.Services;
using AuthenticationAPI.Infrastructure.Caching;
using AuthenticationAPI.Infrastructure.Data;
using AuthenticationAPI.Infrastructure.Repositories;
using BrightNess.Library.DI;
using BrightNess.Library.Security;
using BrightNess.Library.RateLimiting;
using BrightNess.Library.HealthChecks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace AuthenticationAPI.Infrastructure.DependencyInjection
{
    public static class ServiceContainer
    {
        public static IServiceCollection AddInfrastructureService(this IServiceCollection services, IConfiguration config)
        {
            SharedServiceContainer.AddSharedServices<AuthenticationDbContext>(
                services,
                config,
                config["MySerilog:FileName"] ?? "AuthenticationApi",
                schema: "authentication");

            ConfigureRedis(services, config);
            ConfigureSecurityServices(services, config);
            ConfigureHealthChecks(services);

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IUserService, UserService>();

            return services;
        }

        public static IApplicationBuilder UserInfrastructurePolicy(this IApplicationBuilder app)
        {
            SharedServiceContainer.UseSharedPolicies(app);
            
            // Add health checks endpoint
            app.UseHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    var result = JsonSerializer.Serialize(new
                    {
                        status = report.Status.ToString(),
                        checks = report.Entries.Select(entry => new
                        {
                            name = entry.Key,
                            status = entry.Value.Status.ToString(),
                            exception = entry.Value.Exception?.Message,
                            duration = entry.Value.Duration.ToString(),
                            data = entry.Value.Data
                        })
                    });
                    await context.Response.WriteAsync(result);
                }
            });
            
            return app;
        }

        private static void ConfigureRedis(IServiceCollection services, IConfiguration configuration)
        {
            var redisConnection = configuration.GetConnectionString("Redis") ?? configuration["Redis:ConnectionString"];

            if (string.IsNullOrWhiteSpace(redisConnection))
            {
                services.AddSingleton<IRefreshTokenCacheService, NoOpRefreshTokenCacheService>();
                services.AddSingleton<IJwtBlacklistService, NoOpJwtBlacklistService>();
                services.AddSingleton<IRateLimitService, NoOpRateLimitService>();
                return;
            }

            try
            {
                var connectionMultiplexer = ConnectionMultiplexer.Connect(redisConnection);
                services.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer);
                services.AddSingleton<IRefreshTokenCacheService, RefreshTokenCacheService>();
                services.AddSingleton<IJwtBlacklistService, RedisJwtBlacklistService>();
                services.AddSingleton<IRateLimitService, RedisRateLimitService>();
            }
            catch (Exception ex)
            {
                // Log the error and fall back to no-op services
                using var serviceProvider = services.BuildServiceProvider();
                var logger = serviceProvider.GetService<ILogger<RedisRateLimitService>>();
                logger?.LogError(ex, "Failed to connect to Redis, falling back to no-op services");
                
                services.AddSingleton<IRefreshTokenCacheService, NoOpRefreshTokenCacheService>();
                services.AddSingleton<IJwtBlacklistService, NoOpJwtBlacklistService>();
                services.AddSingleton<IRateLimitService, NoOpRateLimitService>();
            }
        }

        private static void ConfigureSecurityServices(IServiceCollection services, IConfiguration configuration)
        {
            // Rate limiting configuration
            services.Configure<RateLimitOptions>(options =>
            {
                options.LoginAttempts = configuration.GetValue<int>("RateLimit:LoginAttempts", 5);
                options.LoginWindow = TimeSpan.FromMinutes(configuration.GetValue<int>("RateLimit:LoginWindowMinutes", 15));
                options.RefreshTokenAttempts = configuration.GetValue<int>("RateLimit:RefreshTokenAttempts", 10);
                options.RefreshTokenWindow = TimeSpan.FromMinutes(configuration.GetValue<int>("RateLimit:RefreshTokenWindowMinutes", 5));
            });
        }

        private static void ConfigureHealthChecks(IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddCheck<RedisHealthCheck>("redis", tags: new[] { "ready", "redis" })
                .AddDbContextCheck<AuthenticationDbContext>("database", tags: new[] { "ready", "database" });
        }
    }

    public class RateLimitOptions
    {
        public int LoginAttempts { get; set; } = 5;
        public TimeSpan LoginWindow { get; set; } = TimeSpan.FromMinutes(15);
        public int RefreshTokenAttempts { get; set; } = 10;
        public TimeSpan RefreshTokenWindow { get; set; } = TimeSpan.FromMinutes(5);
    }

    // No-op rate limit service for fallback
    public class NoOpRateLimitService : IRateLimitService
    {
        public Task<RateLimitResult> CheckRateLimitAsync(string key, int limit, TimeSpan window)
        {
            return Task.FromResult(new RateLimitResult
            {
                IsAllowed = true,
                RemainingRequests = limit,
                RetryAfter = TimeSpan.Zero,
                WindowResetTime = DateTime.UtcNow.Add(window)
            });
        }

        public Task<RateLimitResult> CheckUserRateLimitAsync(string userId, string action, int limit, TimeSpan window)
        {
            return CheckRateLimitAsync($"user:{userId}:{action}", limit, window);
        }
    }
}
