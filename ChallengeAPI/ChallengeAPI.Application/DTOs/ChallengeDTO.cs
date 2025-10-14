using System;
using System.ComponentModel.DataAnnotations;

namespace ChallengeAPI.Application.DTOs
{
    public record ChallengeDTO
    (
         Guid Id,
        Guid HostUserId,

        [Required]
        [StringLength(100, MinimumLength = 1)]
        string Title,

        [StringLength(500)]
        string? Description,

        DateTime? StartAt,

        DateTime? EndAt,

        [Required]
        string Status,

        [Range(1, int.MaxValue)]
        int? MaxParticipants,

        [Range(0.01, (double)decimal.MaxValue)]
        decimal? PrizePool,

        string? Category,

        string? TaskSummary,

        DateTime CreatedAt,

        DateTime? UpdatedAt
    );
}

