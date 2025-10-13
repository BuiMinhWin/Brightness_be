using System;

namespace ChatService.Application.DTOs
{
    public record SendMessageRequest
    (
        Guid? SessionId,
        string Message
    );
}

