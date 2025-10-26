using System;

namespace PersonalCoachAPI.Domain.Entities
{
    public class BrokenLinkReport
    {
        public Guid Id { get; set; }
        public Guid ExerciseId { get; set; }
        public string ExerciseName { get; set; } = string.Empty;
        public string BrokenVideoUrl { get; set; } = string.Empty;
        public Guid ReportedByUserId { get; set; }
        public DateTime ReportedAt { get; set; }
        public string Status { get; set; } = "pending"; // pending, resolved, dismissed
        public string? AdminNotes { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public Guid? ResolvedByUserId { get; set; }
        public string? NewVideoUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

