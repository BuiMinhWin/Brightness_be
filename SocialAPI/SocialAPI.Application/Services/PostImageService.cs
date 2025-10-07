using Microsoft.AspNetCore.Http;
using SocialAPI.Application.DTOs;
using SocialAPI.Application.Interfaces;
using SocialAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SocialAPI.Application.Services
{
    public class PostImageService : IPostImageService
    {
        private readonly IPostImageRepository _postImageRepository;
        private readonly IPostRepository _postRepository;

        public PostImageService(IPostImageRepository postImageRepository, IPostRepository postRepository)
        {
            _postImageRepository = postImageRepository;
            _postRepository = postRepository;
        }

        public async Task<PostImage> CreatePostImageAsync(PostImage postImage)
        {
            return await _postImageRepository.CreatePostImageAsync(postImage);
        }

        public async Task<IEnumerable<PostImageDTO>> GetPostImagesAsync(Guid postId)
        {
            var postImages = await _postImageRepository.GetPostImagesByPostIdAsync(postId);
            return postImages.Select(pi => new PostImageDTO
            {
                Id = pi.Id,
                PostId = pi.PostId,
                FileName = pi.FileName,
                ContentType = pi.ContentType,
                CreatedAt = pi.CreatedAt
            });
        }

        public async Task<PostImageDetailDTO> GetPostImageByIdAsync(Guid imageId)
        {
            var postImage = await _postImageRepository.GetPostImageByIdAsync(imageId);
            if (postImage == null)
            {
                return null;
            }

            return new PostImageDetailDTO
            {
                Id = postImage.Id,
                PostId = postImage.PostId,
                ImageData = postImage.ImageData,
                FileName = postImage.FileName,
                ContentType = postImage.ContentType,
                CreatedAt = postImage.CreatedAt
            };
        }

        public async Task<PostImageDTO> UploadImageAsync(Guid postId, IFormFile imageFile)
        {
            var post = await _postRepository.GetPostByIdAsync(postId);
            if (post == null)
            {
                throw new Exception("Post not found");
            }

            if (imageFile == null || imageFile.Length == 0)
            {
                throw new Exception("Image file is not valid");
            }

            using var memoryStream = new MemoryStream();
            await imageFile.CopyToAsync(memoryStream);

            var postImage = new PostImage
            {
                Id = Guid.NewGuid(),
                PostId = postId,
                ImageData = memoryStream.ToArray(),
                FileName = imageFile.FileName,
                ContentType = imageFile.ContentType,
                CreatedAt = DateTime.UtcNow
            };

            var createdImage = await _postImageRepository.CreatePostImageAsync(postImage);

            return new PostImageDTO
            {
                Id = createdImage.Id,
                PostId = createdImage.PostId,
                FileName = createdImage.FileName,
                ContentType = createdImage.ContentType,
                CreatedAt = createdImage.CreatedAt
            };
        }

        public async Task DeletePostImageAsync(Guid imageId)
        {
            var image = await _postImageRepository.GetPostImageByIdAsync(imageId);
            if (image == null)
            {
                throw new Exception("Image not found");
            }

            await _postImageRepository.DeletePostImageAsync(imageId);
        }
    }
}