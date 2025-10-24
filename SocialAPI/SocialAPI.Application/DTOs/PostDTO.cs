using System;

namespace SocialAPI.Application.DTOs
{
    public record PostDTO(
    Guid Id,
    Guid UserId,
    string Title,
    string Content,
    string Visibility,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    int ReactionCount,
    int DislikeCount,
    int CommentCount,
    UserVoteStatusDTO? UserVoteStatus = null
);

    public record UserVoteStatusDTO(
        bool HasVoted,
        string? VoteType
    );
}
