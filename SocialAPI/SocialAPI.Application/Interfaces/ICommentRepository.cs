using SocialAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SocialAPI.Application.Interfaces
{
    public interface ICommentRepository
    {
        Task<IEnumerable<Comment>> GetCommentsByPostIdAsync(Guid postId);
        Task<Comment> GetCommentByIdAsync(Guid commentId);
        Task<Comment> CreateCommentAsync(Comment comment);
        Task UpdateCommentAsync(Comment comment);
        Task DeleteCommentAsync(Guid commentId);

        Task<CommentVote> GetCommentVoteAsync(Guid commentId, Guid userId);
        Task AddCommentVoteAsync(CommentVote vote);
        Task UpdateCommentVoteAsync(CommentVote vote);
        Task RemoveCommentVoteAsync(CommentVote vote);
        Task<IEnumerable<CommentVote>> GetVotesForCommentAsync(Guid commentId);
        Task UpdateCommentReactionCountAsync(Guid commentId, int reactionCount);
    }
}
