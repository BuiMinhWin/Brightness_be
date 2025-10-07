using System;

namespace SocialAPI.Application.DTOs
{
    public record CreateReportDTO
    (
        string TargetType,
        Guid TargetId,
        string Reason
    );
}
