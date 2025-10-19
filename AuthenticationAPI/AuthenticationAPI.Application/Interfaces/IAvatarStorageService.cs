using System;
using System.Threading.Tasks;

namespace AuthenticationAPI.Application.Interfaces
{
    public interface IAvatarStorageService
    {
        Task<string> SaveAvatarAsync(Guid userId, string dataUrl);
        Task DeleteAvatarAsync(string? existingPath);
    }
}
