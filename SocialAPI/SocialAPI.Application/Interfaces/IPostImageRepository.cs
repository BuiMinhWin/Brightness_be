using SocialAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SocialAPI.Application.Interfaces
{
    public interface IPostImageRepository
    {
        Task<IEnumerable<PostImage>> GetPostImagesByPostIdAsync(Guid postId);
        Task<PostImage> GetPostImageByIdAsync(Guid imageId);
        Task<PostImage> CreatePostImageAsync(PostImage postImage);
        Task DeletePostImageAsync(Guid imageId);
    }
}
