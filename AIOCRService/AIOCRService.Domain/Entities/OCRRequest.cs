using System.ComponentModel.DataAnnotations;

namespace AIOCRService.Domain.Entities
{
    public class OCRRequest
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string ImageBase64 { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public OCRType Type { get; set; }
        public string Status { get; set; } = "pending"; // pending, processing, completed, failed
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public enum OCRType
    {
        FoodLabel = 1,              // Nutrition facts labels
        MedicalDocument = 2,        // General medical documents
        Prescription = 3,           // Medication prescriptions
        LabResult = 4,              // InBody, blood tests, DEXA scans
        InjuryHistory = 5,          // Injury/surgery records
        FitnessAssessment = 6,      // Fitness test results (VO2 max, 1RM, etc.)
        MealPlan = 7,               // Meal plans from nutritionists
        WorkoutLog = 8,             // Previous workout logs
        HealthQuestionnaire = 9,    // Health history forms
        ProgressPhoto = 10,         // Body progress photos
        AllergyTest = 11,           // Allergy/intolerance test results
        SupplementLabel = 12,       // Supplement facts
        General = 99                // Other documents
    }
}

