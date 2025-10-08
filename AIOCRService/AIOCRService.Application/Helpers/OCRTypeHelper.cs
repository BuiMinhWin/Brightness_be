using AIOCRService.Domain.Entities;

namespace AIOCRService.Application.Helpers
{
    public static class OCRTypeHelper
    {
        public static string ToDataType(this OCRType type)
        {
            return type switch
            {
                OCRType.FoodLabel => "food_label",
                OCRType.MedicalDocument => "medical_document",
                OCRType.Prescription => "prescription",
                OCRType.LabResult => "lab_result",
                OCRType.InjuryHistory => "injury_history",
                OCRType.FitnessAssessment => "fitness_assessment",
                OCRType.MealPlan => "meal_plan",
                OCRType.WorkoutLog => "workout_log",
                OCRType.HealthQuestionnaire => "health_questionnaire",
                OCRType.ProgressPhoto => "progress_photo",
                OCRType.AllergyTest => "allergy_test",
                OCRType.SupplementLabel => "supplement_label",
                _ => "general"
            };
        }

        public static string GetDisplayName(this OCRType type)
        {
            return type switch
            {
                OCRType.FoodLabel => "Nhãn Dinh Dưỡng",
                OCRType.MedicalDocument => "Tài Liệu Y Tế",
                OCRType.Prescription => "Đơn Thuốc",
                OCRType.LabResult => "Kết Quả Xét Nghiệm",
                OCRType.InjuryHistory => "Lịch Sử Chấn Thương",
                OCRType.FitnessAssessment => "Đánh Giá Thể Lực",
                OCRType.MealPlan => "Thực Đơn",
                OCRType.WorkoutLog => "Nhật Ký Tập Luyện",
                OCRType.HealthQuestionnaire => "Bảng Câu Hỏi Sức Khỏe",
                OCRType.ProgressPhoto => "Ảnh Tiến Triển",
                OCRType.AllergyTest => "Test Dị Ứng",
                OCRType.SupplementLabel => "Nhãn Thực Phẩm Bổ Sung",
                _ => "Tài Liệu Khác"
            };
        }

        public static string GetRecommendedPrompt(this OCRType type)
        {
            return type switch
            {
                OCRType.FoodLabel => "Phân tích thông tin dinh dưỡng và đánh giá độ lành mạnh",
                OCRType.MedicalDocument => "Phân tích tài liệu y tế và tóm tắt các phát hiện quan trọng",
                OCRType.Prescription => "Phân tích đơn thuốc và cung cấp thông tin về cách sử dụng",
                OCRType.LabResult => "Phân tích InBody/xét nghiệm và đưa ra đánh giá sức khỏe",
                OCRType.InjuryHistory => "Phân tích lịch sử chấn thương và đưa ra khuyến nghị phục hồi",
                OCRType.FitnessAssessment => "Phân tích kết quả đánh giá thể lực và xác định mức độ",
                OCRType.MealPlan => "Phân tích thực đơn và đánh giá cân bằng dinh dưỡng",
                OCRType.WorkoutLog => "Phân tích nhật ký tập luyện và đánh giá tiến bộ",
                OCRType.HealthQuestionnaire => "Phân tích thông tin sức khỏe và xác định rủi ro",
                OCRType.ProgressPhoto => "Phân tích ảnh tiến triển và nhận xét thay đổi cơ thể",
                OCRType.AllergyTest => "Phân tích kết quả test dị ứng và liệt kê hạn chế",
                OCRType.SupplementLabel => "Phân tích thành phần và lợi ích của thực phẩm bổ sung",
                _ => "Phân tích tài liệu"
            };
        }

        public static bool RequiresVisionAI(this OCRType type)
        {
            // Some types work better with direct vision AI
            return type switch
            {
                OCRType.LabResult => true,        // InBody has complex layout
                OCRType.ProgressPhoto => true,    // Visual analysis needed
                OCRType.FoodLabel => true,        // Tables and charts
                OCRType.MealPlan => true,         // Visual meal presentations
                _ => false
            };
        }
    }
}

