using Microsoft.AspNetCore.Http;
using SocialAPI.Application.DTOs;
using SocialAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SocialAPI.Application.Interfaces
{
    public interface IPostImageService
    {
        Task<PostImage> CreatePostImageAsync(PostImage postImage);
        Task<IEnumerable<PostImageDTO>> GetPostImagesAsync(Guid postId);
        Task<PostImageDetailDTO> GetPostImageByIdAsync(Guid imageId);
        Task<PostImageDTO> UploadImageAsync(Guid postId, IFormFile imageFile);
        Task DeletePostImageAsync(Guid imageId);
    }
}
