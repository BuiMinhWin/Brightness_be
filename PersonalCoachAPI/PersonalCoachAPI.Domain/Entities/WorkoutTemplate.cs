using System;
using System.Collections.Generic;

namespace PersonalCoachAPI.Domain.Entities
{
    public class WorkoutTemplate
    {
        public Guid Id { get; set; }
        public string TemplateName { get; set; } = string.Empty;
        public string FocusArea { get; set; } = string.Empty; // upper_body, lower_body, cardio, full_body
        public string Difficulty { get; set; } = string.Empty; // beginner, intermediate, advanced
        public int EstimatedDuration { get; set; } // in minutes
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        
        public ICollection<WorkoutTemplateExercise> Exercises { get; set; } = new List<WorkoutTemplateExercise>();
    }
    
    public class WorkoutTemplateExercise
    {
        public Guid Id { get; set; }
        public Guid TemplateId { get; set; }
        public Guid ExerciseLibraryId { get; set; }
        public int OrderIndex { get; set; }
        public int Sets { get; set; }
        public string Reps { get; set; } = string.Empty;
        public int RestSeconds { get; set; }
        public string Notes { get; set; } = string.Empty;
        
        // Navigation properties
        public WorkoutTemplate Template { get; set; } = null!;
        public ExerciseLibrary Exercise { get; set; } = null!;
    }
}




