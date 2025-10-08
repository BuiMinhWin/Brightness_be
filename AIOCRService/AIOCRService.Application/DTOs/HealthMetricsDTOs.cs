namespace AIOCRService.Application.DTOs
{
    // InBody & Lab Results
    public class InBodyMetrics
    {
        public decimal? Weight { get; set; }
        public decimal? BMI { get; set; }
        public decimal? BodyFatPercent { get; set; }
        public decimal? MuscleMass { get; set; }
        public int? BMR { get; set; }
        public decimal? VisceralFat { get; set; }
        public decimal? TotalBodyWater { get; set; }
        public string Assessment { get; set; } = string.Empty;
    }

    // Nutrition Labels & Food
    public class NutritionMetrics
    {
        public decimal? Calories { get; set; }
        public decimal? Protein { get; set; }
        public decimal? Carbs { get; set; }
        public decimal? Fat { get; set; }
        public decimal? Fiber { get; set; }
        public decimal? Sugar { get; set; }
        public decimal? Sodium { get; set; }
        public int? HealthScore { get; set; }
        public List<string> KeyNutrients { get; set; } = new();
    }

    // Medical Documents
    public class MedicalMetrics
    {
        public List<string> KeyFindings { get; set; } = new();
        public List<string> Conditions { get; set; } = new();
        public string Urgency { get; set; } = string.Empty;
        public List<string> NextSteps { get; set; } = new();
    }

    // Prescriptions
    public class PrescriptionMetrics
    {
        public List<MedicationInfo> Medications { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public List<string> SideEffects { get; set; } = new();
    }

    public class MedicationInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
    }

    // Injury History
    public class InjuryMetrics
    {
        public List<InjuryInfo> Injuries { get; set; } = new();
        public List<string> Restrictions { get; set; } = new();
        public List<string> RecoveryRecommendations { get; set; } = new();
    }

    public class InjuryInfo
    {
        public string Type { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
    }

    // Fitness Assessment
    public class FitnessMetrics
    {
        public List<FitnessTest> Tests { get; set; } = new();
        public List<string> Strengths { get; set; } = new();
        public List<string> Weaknesses { get; set; } = new();
        public string OverallLevel { get; set; } = string.Empty;
    }

    public class FitnessTest
    {
        public string Name { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
        public string Rating { get; set; } = string.Empty;
    }

    // Meal Plan
    public class MealPlanMetrics
    {
        public decimal? TotalCalories { get; set; }
        public MacroNutrients? Macros { get; set; }
        public List<MealInfo> Meals { get; set; } = new();
        public List<string> Analysis { get; set; } = new();
    }

    public class MacroNutrients
    {
        public decimal? Protein { get; set; }
        public decimal? Carbs { get; set; }
        public decimal? Fat { get; set; }
    }

    public class MealInfo
    {
        public string Name { get; set; } = string.Empty;
        public List<string> Foods { get; set; } = new();
    }

    // Workout Log
    public class WorkoutLogMetrics
    {
        public List<ExerciseLog> Exercises { get; set; } = new();
        public string TotalVolume { get; set; } = string.Empty;
        public List<string> Analysis { get; set; } = new();
        public List<string> Progress { get; set; } = new();
    }

    public class ExerciseLog
    {
        public string Name { get; set; } = string.Empty;
        public string Sets { get; set; } = string.Empty;
        public string Reps { get; set; } = string.Empty;
        public string Weight { get; set; } = string.Empty;
    }

    // Health Questionnaire
    public class HealthQuestionnaireMetrics
    {
        public List<string> HealthConditions { get; set; } = new();
        public List<string> CurrentMedications { get; set; } = new();
        public List<string> Allergies { get; set; } = new();
        public LifestyleInfo? Lifestyle { get; set; }
        public List<string> RiskFactors { get; set; } = new();
    }

    public class LifestyleInfo
    {
        public string SleepHours { get; set; } = string.Empty;
        public string StressLevel { get; set; } = string.Empty;
        public string ActivityLevel { get; set; } = string.Empty;
    }

    // Progress Photo
    public class ProgressPhotoMetrics
    {
        public List<string> Observations { get; set; } = new();
        public List<string> Changes { get; set; } = new();
        public BodyAreaAnalysis? BodyAreas { get; set; }
    }

    public class BodyAreaAnalysis
    {
        public string Chest { get; set; } = string.Empty;
        public string Arms { get; set; } = string.Empty;
        public string Core { get; set; } = string.Empty;
        public string Legs { get; set; } = string.Empty;
    }

    // Allergy Test
    public class AllergyMetrics
    {
        public List<AllergyInfo> Allergies { get; set; } = new();
        public List<string> FoodRestrictions { get; set; } = new();
        public List<string> Alternatives { get; set; } = new();
    }

    public class AllergyInfo
    {
        public string Allergen { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Reaction { get; set; } = string.Empty;
    }

    // Supplement Label
    public class SupplementMetrics
    {
        public List<IngredientInfo> Ingredients { get; set; } = new();
        public List<string> Benefits { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public string RecommendedTiming { get; set; } = string.Empty;
    }

    public class IngredientInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
        public string DailyValue { get; set; } = string.Empty;
    }

    // Enhanced DTO with all possible metrics
    public class ProcessedDataWithMetrics : ProcessedDataDTO
    {
        public InBodyMetrics? InBodyData { get; set; }
        public NutritionMetrics? NutritionData { get; set; }
        public MedicalMetrics? MedicalData { get; set; }
        public PrescriptionMetrics? PrescriptionData { get; set; }
        public InjuryMetrics? InjuryData { get; set; }
        public FitnessMetrics? FitnessData { get; set; }
        public MealPlanMetrics? MealPlanData { get; set; }
        public WorkoutLogMetrics? WorkoutLogData { get; set; }
        public HealthQuestionnaireMetrics? QuestionnaireData { get; set; }
        public ProgressPhotoMetrics? ProgressPhotoData { get; set; }
        public AllergyMetrics? AllergyData { get; set; }
        public SupplementMetrics? SupplementData { get; set; }
    }
}

