using System;
using System.Collections.Generic;

namespace PersonalCoachAPI.Application.DTOs
{
    public class ReportBrokenLinkRequest
    {
        public Guid ExerciseId { get; set; }
        public string ExerciseName { get; set; } = string.Empty;
        public string BrokenVideoUrl { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string? UserComment { get; set; }
    }

    public class BrokenLinkReportResponse
    {
        public Guid Id { get; set; }
        public Guid ExerciseId { get; set; }
        public string ExerciseName { get; set; } = string.Empty;
        public string BrokenVideoUrl { get; set; } = string.Empty;
        public Guid ReportedByUserId { get; set; }
        public DateTime ReportedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? AdminNotes { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public Guid? ResolvedByUserId { get; set; }
        public string? NewVideoUrl { get; set; }
    }

    public class BrokenLinkReportListItem
    {
        public Guid Id { get; set; }
        public Guid ExerciseId { get; set; }
        public string ExerciseName { get; set; } = string.Empty;
        public string BrokenVideoUrl { get; set; } = string.Empty;
        public DateTime ReportedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? NewVideoUrl { get; set; }
        public int ReportCount { get; set; } // Number of reports for this exercise
    }

    public class UpdateBrokenLinkRequest
    {
        public string? Status { get; set; }
        public string? AdminNotes { get; set; }
        public string? NewVideoUrl { get; set; }
    }

    public class ResolveBrokenLinkRequest
    {
        public string NewVideoUrl { get; set; } = string.Empty;
        public string? AdminNotes { get; set; }
        public Guid AdminUserId { get; set; }
    }
}

