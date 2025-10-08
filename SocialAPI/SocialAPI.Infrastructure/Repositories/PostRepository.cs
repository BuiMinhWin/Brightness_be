using Microsoft.EntityFrameworkCore;
using SocialAPI.Application.Interfaces;
using SocialAPI.Domain.Entities;
using SocialAPI.Infrastructure.Data;

namespace SocialAPI.Infrastructure.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly SocialDbContext _context;

        public PostRepository(SocialDbContext context)
        {
            _context = context;
        }

        public async Task<Post> CreatePostAsync(Post post)
        {
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            return post;
        }

        public async Task DeletePostAsync(Guid id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Post> GetPostByIdAsync(Guid id)
        {
            return await _context.Posts.FindAsync(id);
        }

        public async Task<IEnumerable<Post>> GetPostsAsync()
        {
            return await _context.Posts.ToListAsync();
        }

        public async Task UpdatePostAsync(Post post)
        {
            _context.Entry(post).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<PostVote> GetPostVoteAsync(Guid postId, Guid userId)
        {
            return await _context.PostVotes.FirstOrDefaultAsync(pv => pv.PostId == postId && pv.UserId == userId);
        }

        public async Task AddPostVoteAsync(PostVote vote)
        {
            _context.PostVotes.Add(vote);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePostVoteAsync(PostVote vote)
        {
            _context.PostVotes.Update(vote);
            await _context.SaveChangesAsync();
        }

        public async Task RemovePostVoteAsync(PostVote vote)
        {
            _context.PostVotes.Remove(vote);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<PostVote>> GetVotesForPostAsync(Guid postId)
        {
            return await _context.PostVotes.Where(pv => pv.PostId == postId).ToListAsync();
        }

        public async Task<IEnumerable<Post>> GetPostsByTagAsync(string tagSlug)
        {
            return await _context.Posts
                .Where(p => p.PostTags.Any(pt => pt.Tag.Slug == tagSlug))
                .ToListAsync();
        }

        public async Task UpdatePostReactionCountAsync(Guid postId, int reactionCount)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post != null)
            {
                post.ReactionCount = reactionCount;
                _context.Posts.Update(post);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdatePostCountsAsync(Guid postId, int reactionCount, int dislikeCount)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post != null)
            {
                post.ReactionCount = reactionCount;
                post.DislikeCount = dislikeCount;
                _context.Posts.Update(post);
                await _context.SaveChangesAsync();
            }
        }
    }
}
