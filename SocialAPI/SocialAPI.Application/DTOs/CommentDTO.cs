using System;

namespace SocialAPI.Application.DTOs
{
    public record CommentDTO
    (
        Guid Id,
        Guid PostId,
        Guid UserId,
        Guid? ParentCommentId,
        string Content,
        string Status,
        DateTime CreatedAt,
        DateTime? UpdatedAt,
        int ReactionCount = 0
    );
}
