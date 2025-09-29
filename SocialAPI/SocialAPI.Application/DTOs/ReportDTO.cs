using System;

namespace SocialAPI.Application.DTOs
{
    public record ReportDTO
    (
        Guid Id,
        Guid ReporterId,
        string TargetType,
        Guid TargetId,
        string Reason,
        string Status,
        string? Notes,
        Guid? ReviewerId,
        DateTime CreatedAt,
        DateTime? ReviewedAt
    );
}
