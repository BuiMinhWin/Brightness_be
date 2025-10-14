using AuthenticationAPI.Application.Interfaces;
using AuthenticationAPI.Application.Services;
using AuthenticationAPI.Infrastructure.Caching;
using AuthenticationAPI.Infrastructure.Data;
using AuthenticationAPI.Infrastructure.Repositories;
using BrightNess.Library.DI;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace AuthenticationAPI.Infrastructure.DependencyInjection
{
    public static class ServiceContainer
    {
        public static IServiceCollection AddInfrastructureService(this IServiceCollection services, IConfiguration config)
        {
            SharedServiceContainer.AddSharedServices<AuthenticationDbContext>(services, config, config["MySerilog:FileName"] ?? "AuthenticationApi");

            ConfigureRedis(services, config);

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IUserService, UserService>();

            return services;
        }

        public static IApplicationBuilder UserInfrastructurePolicy(this IApplicationBuilder app)
        {
            SharedServiceContainer.UseSharedPolicies(app);
            return app;
        }

        private static void ConfigureRedis(IServiceCollection services, IConfiguration configuration)
        {
            var redisConnection = configuration.GetConnectionString("Redis") ?? configuration["Redis:ConnectionString"];

            if (string.IsNullOrWhiteSpace(redisConnection))
            {
                services.AddSingleton<IRefreshTokenCacheService, NoOpRefreshTokenCacheService>();
                return;
            }

            services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnection));
            services.AddSingleton<IRefreshTokenCacheService, RefreshTokenCacheService>();
        }
    }
}
