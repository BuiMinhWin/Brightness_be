using System;

namespace ChatService.Application.DTOs
{
    public record ChatMessageDTO
    (
        Guid Id,
        Guid SessionId,
        Guid UserId,
        string Role,
        string Content,
        int? TokenCount,
        DateTime CreatedAt
    );
}

