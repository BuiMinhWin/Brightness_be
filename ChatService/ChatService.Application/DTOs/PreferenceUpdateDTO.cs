using System.ComponentModel.DataAnnotations;

namespace ChatService.Application.DTOs
{
    public class PreferenceUpdateDTO
    {
        [Required]
        public Guid SessionId { get; set; }
        
        [RegularExpression(@"^(morning|afternoon|evening)$")]
        public string? PreferredWorkoutTime { get; set; }
        
        [Range(1, 7)]
        public int? WorkoutDaysPerWeek { get; set; }
        
        [MaxLength(500)]
        public string? AdditionalNotes { get; set; }
        
        // Only these 3 fields can be updated via chat
        // No SubscriptionPlanId, No UserId, No Role fields
    }
}

