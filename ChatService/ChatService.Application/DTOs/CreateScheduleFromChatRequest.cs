using System.ComponentModel.DataAnnotations;

namespace ChatService.Application.DTOs
{
    public class CreateScheduleFromChatRequest
    {
        [Required]
        public Guid SessionId { get; set; }
        
        [Required]
        [RegularExpression(@"^(weight_loss|muscle_gain|strength|endurance|flexibility)$")]
        public string Goal { get; set; } = string.Empty;
        
        [Range(7, 90)]
        public int DurationDays { get; set; } = 7;
        
        [RegularExpression(@"^(beginner|intermediate|advanced)$")]
        public string DifficultyLevel { get; set; } = string.Empty;
        
        [MaxLength(1000)]
        public string? AdditionalNotes { get; set; }
        
        // UserId will NEVER come from this DTO, always from JWT
    }
}

