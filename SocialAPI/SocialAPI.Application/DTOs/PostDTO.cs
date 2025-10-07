using System;

namespace SocialAPI.Application.DTOs
{
    public record PostDTO
    (
        Guid Id,
        Guid UserId,
        string Title,
        string Content,
        string Visibility,
        string Status,
        DateTime CreatedAt,
        DateTime? UpdatedAt,
        int ReactionCount = 0
    );
}
