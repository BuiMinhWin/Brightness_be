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
    public class CommentRepository : ICommentRepository
    {
        private readonly SocialDbContext _context;

        public CommentRepository(SocialDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Comment>> GetCommentsByPostIdAsync(Guid postId)
        {
            return await _context.Comments
                                 .Where(c => c.PostId == postId)
                                 .ToListAsync();
        }

        public async Task<Comment> GetCommentByIdAsync(Guid commentId)
        {
            return await _context.Comments.FindAsync(commentId);
        }

        public async Task<Comment> CreateCommentAsync(Comment comment)
        {
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task UpdateCommentAsync(Comment comment)
        {
            _context.Entry(comment).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCommentAsync(Guid commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment != null)
            {
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<CommentVote> GetCommentVoteAsync(Guid commentId, Guid userId)
        {
            return await _context.CommentVotes.FirstOrDefaultAsync(cv => cv.CommentId == commentId && cv.UserId == userId);
        }

        public async Task AddCommentVoteAsync(CommentVote vote)
        {
            _context.CommentVotes.Add(vote);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCommentVoteAsync(CommentVote vote)
        {
            _context.CommentVotes.Update(vote);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveCommentVoteAsync(CommentVote vote)
        {
            _context.CommentVotes.Remove(vote);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<CommentVote>> GetVotesForCommentAsync(Guid commentId)
        {
            return await _context.CommentVotes.Where(cv => cv.CommentId == commentId).ToListAsync();
        }

        public async Task UpdateCommentReactionCountAsync(Guid commentId, int reactionCount)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment != null)
            {
                comment.ReactionCount = reactionCount;
                _context.Comments.Update(comment);
                await _context.SaveChangesAsync();
            }
        }
    }
}
