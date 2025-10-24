using System;

namespace PersonalCoachAPI.Domain.Entities
{
    public class AIGenerationUsage
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string GenerationType { get; set; } = string.Empty; // "schedule_create", "schedule_regenerate"
        public DateTime CreatedAt { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public Guid? RelatedScheduleId { get; set; }
    }
}




