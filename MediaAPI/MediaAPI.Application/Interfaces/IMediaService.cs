
using MediaAPI.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaAPI.Application.Interfaces
{
    public interface IMediaService
    {
        Task<PaginatedMediaResponse> GetAllVideosAsync(int pageNumber, int pageSize);
        Task<MediaItemDTO?> GetVideoByIdAsync(Guid id);
        Task<MediaItemDTO> CreateVideoAsync(CreateMediaItemRequest createRequest);
        Task<bool> UpdateVideoAsync(Guid id, UpdateMediaItemRequest updateRequest);
        Task<bool> DeleteVideoAsync(Guid id);
        Task<bool> LikeVideoAsync(Guid id, Guid userId);
        Task<bool> UnlikeVideoAsync(Guid id, Guid userId);
        Task<bool> ViewVideoAsync(Guid id, Guid userId);
        Task<Stream> GetVideoFileAsync(Guid id);
    }
}
