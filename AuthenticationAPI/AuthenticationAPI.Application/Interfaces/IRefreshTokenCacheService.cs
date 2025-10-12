using System;
using System.Threading.Tasks;
using AuthenticationAPI.Domain.Entities;

namespace AuthenticationAPI.Application.Interfaces
{
    public interface IRefreshTokenCacheService
    {
        Task CacheTokenAsync(RefreshToken token);
        Task RemoveTokenAsync(string token);
        Task RemoveTokensByUserAsync(Guid userId);
    }
}
