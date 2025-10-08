using AIOCRService.Application.DTOs;

namespace AIOCRService.Application.Interfaces
{
    public interface IUserHealthDataService
    {
        Task<UserHealthProfile> GetUserHealthProfileAsync(Guid userId);
        Task<IEnumerable<ProcessedDataDTO>> GetUserDataByTypeAsync(Guid userId, string dataType);
        Task<string> BuildUserContextForAIAsync(Guid userId);
    }

    public class UserHealthProfile
    {
        public Guid UserId { get; set; }
        
        // Latest body composition
        public ProcessedDataDTO? LatestInBody { get; set; }
        public ProcessedDataDTO? LatestBloodTest { get; set; }
        
        // Medical info
        public List<ProcessedDataDTO> InjuryHistory { get; set; } = new();
        public List<ProcessedDataDTO> AllergiesIntolerances { get; set; } = new();
        public List<ProcessedDataDTO> Medications { get; set; } = new();
        public List<ProcessedDataDTO> HealthConditions { get; set; } = new();
        
        // Fitness data
        public List<ProcessedDataDTO> FitnessAssessments { get; set; } = new();
        public List<ProcessedDataDTO> WorkoutLogs { get; set; } = new();
        public List<ProcessedDataDTO> ProgressPhotos { get; set; } = new();
        
        // Nutrition data
        public List<ProcessedDataDTO> MealPlans { get; set; } = new();
        public List<ProcessedDataDTO> SupplementsUsed { get; set; } = new();
        
        // Summary for AI
        public string HealthSummary { get; set; } = string.Empty;
        public string FitnessSummary { get; set; } = string.Empty;
        public string NutritionSummary { get; set; } = string.Empty;
        public string RestrictionsSummary { get; set; } = string.Empty;
    }
}

