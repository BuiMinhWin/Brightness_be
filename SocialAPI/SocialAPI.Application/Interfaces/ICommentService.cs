using SocialAPI.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SocialAPI.Application.Interfaces
{
    public interface ICommentService
    {
        Task<IEnumerable<CommentDTO>> GetCommentsByPostIdAsync(Guid postId, Guid? userId = null);
        Task<CommentDTO> GetCommentByIdAsync(Guid commentId, Guid? userId = null);
        Task<CommentDTO> CreateCommentAsync(Guid postId, CreateCommentDTO commentDto, Guid userId, string? userName = null);
        Task<CommentDTO> UpdateCommentAsync(Guid commentId, UpdateCommentDTO commentDto, Guid userId);
        Task ToggleCommentVisibilityAsync(Guid commentId, Guid userId);
        Task DeleteCommentAsync(Guid commentId, Guid userId);

        Task VoteCommentAsync(Guid commentId, Guid userId, string voteType);
        Task CancelCommentVoteAsync(Guid commentId, Guid userId);
        Task UpdateCommentReactionCountAsync(Guid commentId);
    }
}
