using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SocialAPI.Application.DTOs;
using SocialAPI.Application.Interfaces;
using SocialAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialAPI.Application.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IPostRepository _postRepository; // To check if post exists
        private readonly IPostService _postService;
        private readonly ILogger<CommentService> _logger;

        public CommentService(ICommentRepository commentRepository, IPostRepository postRepository, IPostService postService, ILogger<CommentService> logger)
        {
            _commentRepository = commentRepository;
            _postRepository = postRepository;
            _postService = postService;
            _logger = logger;
        }

        public async Task<IEnumerable<CommentDTO>> GetCommentsByPostIdAsync(Guid postId, Guid? userId = null)
        {
            var comments = await _commentRepository.GetCommentsByPostIdAsync(postId);
            var commentDtos = new List<CommentDTO>();
            foreach (var comment in comments)
            {
                // Get user vote status if userId is provided
                UserVoteStatusDTO? userVoteStatus = null;
                if (userId.HasValue)
                {
                    var userVote = await _commentRepository.GetCommentVoteAsync(comment.Id, userId.Value);
                    userVoteStatus = userVote != null 
                        ? new UserVoteStatusDTO(true, userVote.VoteType)
                        : new UserVoteStatusDTO(false, null);
                }

                commentDtos.Add(new CommentDTO(comment.Id, comment.PostId, comment.UserId, comment.UserName, comment.ParentCommentId, comment.Content, comment.Status, comment.CreatedAt, comment.UpdatedAt, await CalculateReactions(comment.Id), userVoteStatus));
            }
            return commentDtos;
        }

        public async Task<CommentDTO> GetCommentByIdAsync(Guid commentId, Guid? userId = null)
        {
            var comment = await _commentRepository.GetCommentByIdAsync(commentId);
            if (comment == null) return null;
            
            // Get user vote status if userId is provided
            UserVoteStatusDTO? userVoteStatus = null;
            if (userId.HasValue)
            {
                var userVote = await _commentRepository.GetCommentVoteAsync(commentId, userId.Value);
                userVoteStatus = userVote != null 
                    ? new UserVoteStatusDTO(true, userVote.VoteType)
                    : new UserVoteStatusDTO(false, null);
            }
            
            return new CommentDTO(comment.Id, comment.PostId, comment.UserId, comment.UserName, comment.ParentCommentId, comment.Content, comment.Status, comment.CreatedAt, comment.UpdatedAt, await CalculateReactions(comment.Id), userVoteStatus);
        }

        public async Task<CommentDTO> CreateCommentAsync(Guid postId, CreateCommentDTO commentDto, Guid userId, string? userName = null)
        {
            var post = await _postRepository.GetPostByIdAsync(postId);
            if (post == null) throw new Exception("Post not found");

            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                PostId = postId,
                UserId = userId,
                UserName = userName,
                Content = commentDto.Content,
                ParentCommentId = commentDto.ParentCommentId,
                CreatedAt = DateTime.UtcNow
            };

            var createdComment = await _commentRepository.CreateCommentAsync(comment);
            return new CommentDTO(createdComment.Id, createdComment.PostId, createdComment.UserId, createdComment.UserName, createdComment.ParentCommentId, createdComment.Content, createdComment.Status, createdComment.CreatedAt, createdComment.UpdatedAt, createdComment.ReactionCount, null);
        }

        public async Task<CommentDTO> UpdateCommentAsync(Guid commentId, UpdateCommentDTO commentDto, Guid userId)
        {
            var comment = await _commentRepository.GetCommentByIdAsync(commentId);
            if (comment == null) throw new Exception("Comment not found");
            if (comment.UserId != userId) throw new Exception("User not authorized to update this comment");

            comment.Content = commentDto.Content;
            comment.UpdatedAt = DateTime.UtcNow;

            await _commentRepository.UpdateCommentAsync(comment);

            return new CommentDTO(
              comment.Id,
              comment.PostId,
              comment.UserId,
              comment.UserName,
              comment.ParentCommentId,
              comment.Content,
              comment.Status,
              comment.CreatedAt,
              comment.UpdatedAt,
              await CalculateReactions(comment.Id),
              null
          );
        }

        public async Task ToggleCommentVisibilityAsync(Guid commentId, Guid userId)
        {
            var comment = await _commentRepository.GetCommentByIdAsync(commentId);
            if (comment == null) throw new Exception("Comment not found");

            // Add logic here to check for moderator role if needed
            if (comment.UserId != userId) throw new Exception("User not authorized to change visibility of this comment");

            comment.Status = comment.Status == "visible" ? "hidden" : "visible";
            comment.UpdatedAt = DateTime.UtcNow;

            await _commentRepository.UpdateCommentAsync(comment);
        }

        public async Task DeleteCommentAsync(Guid commentId, Guid userId)
        {
            var comment = await _commentRepository.GetCommentByIdAsync(commentId);
            if (comment == null) throw new Exception("Comment not found");
            if (comment.UserId != userId) throw new Exception("User not authorized to delete this comment");

            await _commentRepository.DeleteCommentAsync(commentId);
        }

                public async Task VoteCommentAsync(Guid commentId, Guid userId, string voteType)
                {
                    var existingVote = await _commentRepository.GetCommentVoteAsync(commentId, userId);
                    if (existingVote != null)
                    {
                        if (existingVote.VoteType == voteType)
                        {
                            // User is casting the same vote again, so we cancel it
                            await _commentRepository.RemoveCommentVoteAsync(existingVote);
                        }
                        else
                        {
                            // User is changing their vote
                            existingVote.VoteType = voteType;
                            await _commentRepository.UpdateCommentVoteAsync(existingVote);
                        }
                    }
                    else
                    {
                        // New vote
                        var newVote = new CommentVote
                        {
                            Id = Guid.NewGuid(),
                            CommentId = commentId,
                            UserId = userId,
                            VoteType = voteType,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _commentRepository.AddCommentVoteAsync(newVote);
                    }
        
                    await UpdateCommentReactionCountAsync(commentId);
                }
        
                public async Task CancelCommentVoteAsync(Guid commentId, Guid userId)
                {
                    var existingVote = await _commentRepository.GetCommentVoteAsync(commentId, userId);
                    if (existingVote != null)
                    {
                        await _commentRepository.RemoveCommentVoteAsync(existingVote);
                    }
        
                    await UpdateCommentReactionCountAsync(commentId);
                }
        
                public async Task UpdateCommentReactionCountAsync(Guid commentId)
                {
                    var reactionCount = await CalculateReactions(commentId);
                    _logger.LogInformation("Updating reaction count for comment {commentId} to {reactionCount}", commentId, reactionCount);
                    await _commentRepository.UpdateCommentReactionCountAsync(commentId, reactionCount);

                    var comment = await _commentRepository.GetCommentByIdAsync(commentId);
                    if (comment != null)
                    {
                        await _postService.UpdatePostReactionCountAsync(comment.PostId);
                    }
                }
                private async Task<int> CalculateReactions(Guid commentId)
                {
                    // Lấy tất cả các lượt vote cho comment
                    var votes = await _commentRepository.GetVotesForCommentAsync(commentId);

                    // Vì bạn chỉ có nút "like", nên chỉ cần đếm số lượng voteType = "like"
                    int likeCount = votes.Count(v => v.VoteType == "like");

                    return likeCount;
                }
    }
}
