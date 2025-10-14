using MediaAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaAPI.Application.Interfaces
{
    public interface IMediaRepository
    {
        Task<IEnumerable<MediaItem>> GetAllAsync(int pageNumber, int pageSize);
        Task<MediaItem?> GetByIdAsync(Guid id);
        Task<MediaItem> AddAsync(MediaItem entity);
        Task<bool> UpdateAsync(MediaItem entity);
        Task<bool> DeleteAsync(Guid id);
        Task<UserLike> AddLikeAsync(UserLike like);
        Task<UserView> AddViewAsync(UserView view);
        Task<UserLike?> GetLikeAsync(Guid userId, Guid mediaItemId);
        Task<UserView?> GetViewAsync(Guid userId, Guid mediaItemId);
        void RemoveLike(UserLike like);
        Task<bool> RemoveLikeAndDecreaseCountAsync(UserLike like, MediaItem mediaItem);
    }
}
