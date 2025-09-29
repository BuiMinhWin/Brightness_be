using System;

namespace SocialAPI.Application.DTOs
{
    public record PostTagDTO
    (
        Guid PostId,
        Guid TagId
    );
}
