using System;

namespace SocialAPI.Domain.Entities
{
    public class Report
    {
        public Guid Id { get; set; }
        public Guid ReporterId { get; set; }
        public string TargetType { get; set; } = null!;
        public Guid TargetId { get; set; }
        public string Reason { get; set; } = null!;
        public string Status { get; set; } = "pending";
        public string? Notes { get; set; }
        public Guid? ReviewerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }
}
