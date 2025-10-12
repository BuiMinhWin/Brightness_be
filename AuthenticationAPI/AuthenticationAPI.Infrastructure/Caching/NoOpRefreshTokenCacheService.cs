using System;
using System.Threading.Tasks;
using AuthenticationAPI.Application.Interfaces;
using AuthenticationAPI.Domain.Entities;

namespace AuthenticationAPI.Infrastructure.Caching
{
    public class NoOpRefreshTokenCacheService : IRefreshTokenCacheService
    {
        public Task CacheTokenAsync(RefreshToken token) => Task.CompletedTask;

        public Task RemoveTokenAsync(string token) => Task.CompletedTask;

        public Task RemoveTokensByUserAsync(Guid userId) => Task.CompletedTask;

    }
}
