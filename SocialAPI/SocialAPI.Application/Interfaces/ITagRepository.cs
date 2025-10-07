using SocialAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SocialAPI.Application.Interfaces
{
    public interface ITagRepository
    {
        Task<IEnumerable<Tag>> GetTagsAsync();
        Task<Tag> GetTagByIdAsync(Guid tagId);
        Task<Tag> GetTagBySlugAsync(string slug);
        Task<Tag> CreateTagAsync(Tag tag);
        Task DeleteTagAsync(Guid tagId);
    }
}
