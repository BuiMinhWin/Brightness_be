using Microsoft.EntityFrameworkCore;
using SocialAPI.Application.Interfaces;
using SocialAPI.Domain.Entities;
using SocialAPI.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialAPI.Infrastructure.Repositories
{
    public class PostImageRepository : IPostImageRepository
    {
        private readonly SocialDbContext _context;

        public PostImageRepository(SocialDbContext context)
        {
            _context = context;
        }

        // Đổi tên từ GetPostImagesAsync thành GetPostImagesByPostIdAsync
        public async Task<IEnumerable<PostImage>> GetPostImagesByPostIdAsync(Guid postId)
        {
            return await _context.PostImages
                                 .Where(pi => pi.PostId == postId)
                                 .ToListAsync();
        }

        public async Task<PostImage> GetPostImageByIdAsync(Guid imageId)
        {
            return await _context.PostImages.FindAsync(imageId);
        }

        public async Task<PostImage> CreatePostImageAsync(PostImage postImage)
        {
            _context.PostImages.Add(postImage);
            await _context.SaveChangesAsync();
            return postImage;
        }

        public async Task DeletePostImageAsync(Guid imageId)
        {
            var postImage = await _context.PostImages.FindAsync(imageId);
            if (postImage != null)
            {
                _context.PostImages.Remove(postImage);
                await _context.SaveChangesAsync();
            }
        }
    }
}