using ChatService.Application.DTOs;

namespace ChatService.Application.Interfaces
{
    public interface IUserContextCacheService
    {
        Task<UserContextDTO?> GetCachedContextAsync(Guid sessionId);
        Task SetCachedContextAsync(Guid sessionId, UserContextDTO context);
        Task InvalidateCacheAsync(Guid sessionId);
    }
}

