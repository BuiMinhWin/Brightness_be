using System;

namespace ChatService.Application.DTOs
{
    public record SendMessageResponse
    (
        Guid SessionId,
        string Message,
        string Role,
        int RemainingMessages,
        int DailyLimit,
        DateTime CreatedAt
    );
}

