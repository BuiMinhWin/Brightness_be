using System;

namespace SocialAPI.Application.DTOs
{
    public record PostVoteDTO
    (
        Guid Id,
        Guid PostId,
        Guid UserId,
        string VoteType,
        DateTime CreatedAt
    );
}
