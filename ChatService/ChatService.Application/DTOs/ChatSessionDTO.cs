using System;

namespace ChatService.Application.DTOs
{
    public record ChatSessionDTO
    (
        Guid Id,
        Guid UserId,
        string? Title,
        string? Summary,
        string Status,
        DateTime CreatedAt,
        DateTime? UpdatedAt
    );
}
