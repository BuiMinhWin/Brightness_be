using AIOCRService.Application.DTOs;
using AIOCRService.Domain.Entities;
using Newtonsoft.Json.Linq;

namespace AIOCRService.Application.Helpers
{
    public static class ProcessedDataMapper
    {
        public static ProcessedDataDTO ToDTO(this ProcessedData entity)
        {
            return new ProcessedDataDTO
            {
                Id = entity.Id,
                OCRResultId = entity.OCRResultId,
                UserId = entity.UserId,
                DataType = entity.DataType,
                ProcessedContent = entity.ProcessedContent,
                Summary = entity.Summary,
                Insights = entity.Insights,
                IsLatest = entity.IsLatest,
                CreatedAt = entity.CreatedAt
            };
        }

        public static ProcessedDataWithMetrics ToEnhancedDTO(this ProcessedData entity)
        {
            var dto = new ProcessedDataWithMetrics
            {
                Id = entity.Id,
                OCRResultId = entity.OCRResultId,
                UserId = entity.UserId,
                DataType = entity.DataType,
                ProcessedContent = entity.ProcessedContent,
                Summary = entity.Summary,
                Insights = entity.Insights,
                IsLatest = entity.IsLatest,
                CreatedAt = entity.CreatedAt
            };

            // Extract structured metrics based on data type
            if (!string.IsNullOrEmpty(entity.ProcessedContent))
            {
                try
                {
                    var json = JObject.Parse(AIResponseParser.CleanMarkdownCodeBlocks(entity.ProcessedContent));

                    switch (entity.DataType)
                    {
                        case "lab_result":
                            dto.InBodyData = new InBodyMetrics
                            {
                                Weight = GetDecimalValue(json, "weight", "cân_nặng", "can_nang"),
                                BMI = GetDecimalValue(json, "bmi", "BMI"),
                                BodyFatPercent = ParsePercentage(GetStringValue(json, "bodyFat", "body_fat", "tỷ_lệ_mỡ", "ty_le_mo")),
                                MuscleMass = GetDecimalValue(json, "muscle", "muscleMass", "khối_cơ", "khoi_co"),
                                BMR = ParseCalories(GetStringValue(json, "bmr", "BMR")),
                                VisceralFat = GetDecimalValue(json, "visceralFat", "visceral_fat", "mỡ_nội_tạng", "mo_noi_tang"),
                                TotalBodyWater = GetDecimalValue(json, "totalBodyWater", "total_body_water", "nước", "nuoc"),
                                Assessment = GetStringValue(json, "assessment", "đánh_giá", "danh_gia") ?? ""
                            };
                            break;

                        case "food_label":
                            dto.NutritionData = new NutritionMetrics
                            {
                                Calories = json["calories"]?.Value<decimal?>(),
                                Protein = ParseGrams(json["protein"]?.ToString()),
                                Carbs = ParseGrams(json["carbs"]?.ToString()),
                                Fat = ParseGrams(json["fat"]?.ToString()),
                                Fiber = ParseGrams(json["fiber"]?.ToString()),
                                Sugar = ParseGrams(json["sugar"]?.ToString()),
                                Sodium = ParseGrams(json["sodium"]?.ToString()),
                                HealthScore = json["healthScore"]?.Value<int?>(),
                                KeyNutrients = json["keyNutrients"]?.ToObject<List<string>>() ?? new()
                            };
                            break;

                        case "medical_document":
                            dto.MedicalData = new MedicalMetrics
                            {
                                KeyFindings = json["keyFindings"]?.ToObject<List<string>>() ?? new(),
                                Conditions = json["conditions"]?.ToObject<List<string>>() ?? new(),
                                Urgency = json["urgency"]?.ToString() ?? "",
                                NextSteps = json["nextSteps"]?.ToObject<List<string>>() ?? new()
                            };
                            break;

                        case "prescription":
                            dto.PrescriptionData = new PrescriptionMetrics
                            {
                                Medications = json["medications"]?.ToObject<List<MedicationInfo>>() ?? new(),
                                Warnings = json["warnings"]?.ToObject<List<string>>() ?? new(),
                                SideEffects = json["sideEffects"]?.ToObject<List<string>>() ?? new()
                            };
                            break;

                        case "injury_history":
                            dto.InjuryData = new InjuryMetrics
                            {
                                Injuries = json["injuries"]?.ToObject<List<InjuryInfo>>() ?? new(),
                                Restrictions = json["restrictions"]?.ToObject<List<string>>() ?? new(),
                                RecoveryRecommendations = json["recovery"]?.ToObject<List<string>>() ?? new()
                            };
                            break;

                        case "fitness_assessment":
                            dto.FitnessData = new FitnessMetrics
                            {
                                Tests = json["tests"]?.ToObject<List<FitnessTest>>() ?? new(),
                                Strengths = json["strengths"]?.ToObject<List<string>>() ?? new(),
                                Weaknesses = json["weaknesses"]?.ToObject<List<string>>() ?? new(),
                                OverallLevel = json["overallLevel"]?.ToString() ?? ""
                            };
                            break;

                        case "meal_plan":
                            dto.MealPlanData = new MealPlanMetrics
                            {
                                TotalCalories = json["totalCalories"]?.Value<decimal?>(),
                                Macros = json["macros"]?.ToObject<MacroNutrients>(),
                                Meals = json["meals"]?.ToObject<List<MealInfo>>() ?? new(),
                                Analysis = json["analysis"]?.ToObject<List<string>>() ?? new()
                            };
                            break;

                        case "workout_log":
                            dto.WorkoutLogData = new WorkoutLogMetrics
                            {
                                Exercises = json["exercises"]?.ToObject<List<ExerciseLog>>() ?? new(),
                                TotalVolume = json["volume"]?.ToString() ?? "",
                                Analysis = json["analysis"]?.ToObject<List<string>>() ?? new(),
                                Progress = json["progress"]?.ToObject<List<string>>() ?? new()
                            };
                            break;

                        case "health_questionnaire":
                            dto.QuestionnaireData = new HealthQuestionnaireMetrics
                            {
                                HealthConditions = json["healthConditions"]?.ToObject<List<string>>() ?? new(),
                                CurrentMedications = json["medications"]?.ToObject<List<string>>() ?? new(),
                                Allergies = json["allergies"]?.ToObject<List<string>>() ?? new(),
                                Lifestyle = json["lifestyle"]?.ToObject<LifestyleInfo>(),
                                RiskFactors = json["riskFactors"]?.ToObject<List<string>>() ?? new()
                            };
                            break;

                        case "progress_photo":
                            dto.ProgressPhotoData = new ProgressPhotoMetrics
                            {
                                Observations = json["observations"]?.ToObject<List<string>>() ?? new(),
                                Changes = json["changes"]?.ToObject<List<string>>() ?? new(),
                                BodyAreas = json["bodyAreas"]?.ToObject<BodyAreaAnalysis>()
                            };
                            break;

                        case "allergy_test":
                            dto.AllergyData = new AllergyMetrics
                            {
                                Allergies = json["allergies"]?.ToObject<List<AllergyInfo>>() ?? new(),
                                FoodRestrictions = json["restrictions"]?.ToObject<List<string>>() ?? new(),
                                Alternatives = json["alternatives"]?.ToObject<List<string>>() ?? new()
                            };
                            break;

                        case "supplement_label":
                            dto.SupplementData = new SupplementMetrics
                            {
                                Ingredients = json["ingredients"]?.ToObject<List<IngredientInfo>>() ?? new(),
                                Benefits = json["benefits"]?.ToObject<List<string>>() ?? new(),
                                Warnings = json["warnings"]?.ToObject<List<string>>() ?? new(),
                                RecommendedTiming = json["timing"]?.ToString() ?? ""
                            };
                            break;
                    }
                }
                catch
                {
                    // If parsing fails, metrics will be null
                }
            }

            return dto;
        }

        private static string? GetStringValue(JObject json, params string[] fieldNames)
        {
            foreach (var field in fieldNames)
            {
                var value = json[field];
                if (value != null && value.Type == JTokenType.String)
                {
                    return value.ToString();
                }
            }
            return null;
        }

        private static decimal? GetDecimalValue(JObject json, params string[] fieldNames)
        {
            foreach (var field in fieldNames)
            {
                var value = json[field];
                if (value != null)
                {
                    if (value.Type == JTokenType.Float || value.Type == JTokenType.Integer)
                    {
                        return value.Value<decimal?>();
                    }
                    if (value.Type == JTokenType.String)
                    {
                        var str = value.ToString();
                        if (decimal.TryParse(str.Replace(",", "."), out var result))
                        {
                            return result;
                        }
                    }
                }
            }
            return null;
        }

        private static decimal? ParsePercentage(string? value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            
            var cleaned = value.Replace("%", "").Trim();
            if (decimal.TryParse(cleaned, out var result))
                return result;
            
            return null;
        }

        private static decimal? ParseGrams(string? value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            
            var cleaned = value.Replace("g", "").Replace("grams", "").Trim();
            if (decimal.TryParse(cleaned, out var result))
                return result;
            
            return null;
        }

        private static int? ParseCalories(string? value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            
            var cleaned = value.Replace("kcal", "").Replace("cal", "").Trim();
            if (int.TryParse(cleaned, out var result))
                return result;
            
            return null;
        }

        public static OCRRequestDTO ToDTO(this OCRRequest entity)
        {
            return new OCRRequestDTO
            {
                Id = entity.Id,
                UserId = entity.UserId,
                FileName = entity.FileName,
                ContentType = entity.ContentType,
                FileSize = entity.FileSize,
                Type = entity.Type,
                Status = entity.Status,
                CreatedAt = entity.CreatedAt,
                ProcessedAt = entity.ProcessedAt,
                ErrorMessage = entity.ErrorMessage
            };
        }

        public static OCRResultDTO ToDTO(this OCRResult entity)
        {
            return new OCRResultDTO
            {
                Id = entity.Id,
                OCRRequestId = entity.OCRRequestId,
                ExtractedText = entity.ExtractedText,
                StructuredData = entity.StructuredData,
                Confidence = entity.Confidence,
                Language = entity.Language,
                CreatedAt = entity.CreatedAt
            };
        }
    }
}

