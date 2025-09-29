using System;

namespace SocialAPI.Application.DTOs
{
    public record TagDTO
    (
        Guid Id,
        string Slug,
        string Name,
        DateTime CreatedAt
    );
}
