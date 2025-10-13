

using MediaAPI.Application.Interfaces;
using MediaAPI.Domain.Entities;
using MediaAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaAPI.Infrastructure.Repositories
{
    public class MediaRepository : IMediaRepository
    {
        private readonly MediaDbContext _context;

        public MediaRepository(MediaDbContext context)
        {
            _context = context;
        }

        public async Task<MediaItem> AddAsync(MediaItem entity)
        {
            await _context.MediaItems.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _context.MediaItems.FindAsync(id);
            if (entity == null)
            {
                return false;
            }

            _context.MediaItems.Remove(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<MediaItem>> GetAllAsync(int pageNumber, int pageSize)
        {
            return await _context.MediaItems
                .Where(i => i.Status == "active")
                .OrderByDescending(i => i.CreatedAt)
                .Include(i => i.UserLikes)
                .Include(i => i.UserViews)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.MediaItems
                .Where(i => i.Status == "active")
                .CountAsync();
        }

        public async Task<MediaItem?> GetByIdAsync(Guid id)
        {
            return await _context.MediaItems
                .Include(i => i.UserLikes)
                .Include(i => i.UserViews)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<bool> UpdateAsync(MediaItem entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<UserLike> AddLikeAsync(UserLike like)
        {
            await _context.UserLikes.AddAsync(like);
            await _context.SaveChangesAsync();
            return like;
        }

        public async Task<UserView> AddViewAsync(UserView view)
        {
            await _context.UserViews.AddAsync(view);
            await _context.SaveChangesAsync();
            return view;
        }

        public async Task<UserLike?> GetLikeAsync(Guid userId, Guid mediaItemId)
        {
            return await _context.UserLikes.FirstOrDefaultAsync(x => x.UserId == userId && x.MediaItemId == mediaItemId);
        }

        public async Task<UserView?> GetViewAsync(Guid userId, Guid mediaItemId)
        {
            return await _context.UserViews.FirstOrDefaultAsync(x => x.UserId == userId && x.MediaItemId == mediaItemId);
        }

        public void RemoveLike(UserLike like)
        {
            _context.UserLikes.Remove(like);
        }
        public async Task<bool> RemoveLikeAndDecreaseCountAsync(UserLike like, MediaItem mediaItem)
        {
            _context.UserLikes.Remove(like);
            mediaItem.Likes--;

            _context.MediaItems.Update(mediaItem);
            return await _context.SaveChangesAsync() > 0;
        }

    }
}
