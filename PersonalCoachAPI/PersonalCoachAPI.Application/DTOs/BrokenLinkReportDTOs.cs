namespace PersonalCoachAPI.Application.DTOs
{
    public class BrokenLinkReportDTO
    {
        public Guid Id { get; set; }
        public string ExerciseId { get; set; } = string.Empty;
        public string ExerciseName { get; set; } = string.Empty;
        public string BrokenVideoUrl { get; set; } = string.Empty;
        public Guid ReportedByUserId { get; set; }
        public DateTime ReportedAt { get; set; }
        public string Status { get; set; } = "pending";
        public string? UserComment { get; set; }
        public string? AdminNotes { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public Guid? ResolvedByUserId { get; set; }
        public string? NewVideoUrl { get; set; }
    }

    public class BrokenLinkReportListItemDTO
    {
        public Guid Id { get; set; }
        public string ExerciseId { get; set; } = string.Empty;
        public string ExerciseName { get; set; } = string.Empty;
        public string BrokenVideoUrl { get; set; } = string.Empty;
        public DateTime ReportedAt { get; set; }
        public string Status { get; set; } = "pending";
        public string? NewVideoUrl { get; set; }
        public int ReportCount { get; set; }
        public string? AdminNotes { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? UserComment { get; set; }
        public bool IsResolved { get; set; }
    }

    public class CreateBrokenLinkReportRequest
    {
        public string ExerciseId { get; set; } = string.Empty;
        public string ExerciseName { get; set; } = string.Empty;
        public string BrokenVideoUrl { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string? UserComment { get; set; }
    }

    public class UpdateBrokenLinkReportRequest
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

