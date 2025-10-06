using SocialAPI.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SocialAPI.Application.Interfaces
{
    public interface ITagService
    {
        Task<IEnumerable<TagDTO>> GetTagsAsync();
        Task<TagDTO> GetTagByIdAsync(Guid tagId);
        Task<TagDTO> CreateTagAsync(CreateTagDTO tagDto);
        Task DeleteTagAsync(Guid tagId);
    }
}
