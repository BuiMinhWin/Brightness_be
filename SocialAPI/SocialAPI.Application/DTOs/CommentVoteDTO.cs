using System;

namespace SocialAPI.Application.DTOs
{
    public record CommentVoteDTO
    (
        Guid Id,
        Guid CommentId,
        Guid UserId,
        string VoteType,
        DateTime CreatedAt
    );
}
