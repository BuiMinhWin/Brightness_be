namespace AIOCRService.Infrastructure.Helpers
{
    public static class TextOptimizationHelper
    {
        public static string OptimizeForAI(string extractedText, string dataType, int maxChars = 1000)
        {
            if (dataType == "lab_value" || dataType == "lab_result")
            {
                return OptimizeInBodyText(extractedText, maxChars);
            }

            if (dataType == "food_label" || dataType == "nutrition")
            {
                return OptimizeNutritionLabel(extractedText, maxChars);
            }

            // For other types, simple truncation with preserving structure
            return TruncateText(extractedText, maxChars * 3);
        }

        private static string OptimizeInBodyText(string text, int maxChars)
        {
            var lines = text.Split('\n')
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => line.Trim())
                .ToList();

            var importantKeywords = new[] {
                "Cân nặng", "Weight", "lbs", "kg",
                "BMI", "Body Mass Index",
                "PBF", "Percent Body Fat", "Body Fat", "Khối lượng béo",
                "SMM", "Skeletal Muscle Mass", "Khối lượng cơ",
                "BMR", "Basal Metabolic Rate", "Tỷ lệ trao đổi chất",
                "VFA", "Visceral Fat", "Mỡ nội tạng",
                "TBW", "Total Body Water", "Tổng lượng nước",
                "Protein", "Mineral", "Lean Body Mass",
                "Right Arm", "Left Arm", "Trunk", "Right Leg", "Left Leg",
                "Tay phải", "Tay trái", "Thân", "Chân phải", "Chân trái"
            };

            var importantLines = lines
                .Where(line => importantKeywords.Any(keyword =>
                    line.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            var optimized = string.Join("\n", importantLines);
            return TruncateText(optimized, maxChars);
        }

        private static string OptimizeNutritionLabel(string text, int maxChars)
        {
            var lines = text.Split('\n')
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => line.Trim())
                .ToList();

            var nutritionKeywords = new[] {
                "Calories", "Calorie", "kcal", "Năng lượng",
                "Protein", "Đạm", 
                "Carbohydrate", "Carbs", "Tinh bột",
                "Fat", "Mỡ", "Chất béo",
                "Fiber", "Chất xơ",
                "Sugar", "Đường",
                "Sodium", "Natri", "Muối",
                "Vitamin", "Khoáng chất", "Mineral",
                "Serving", "Khẩu phần"
            };

            var importantLines = lines
                .Where(line => nutritionKeywords.Any(keyword =>
                    line.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            var optimized = string.Join("\n", importantLines);
            return TruncateText(optimized, maxChars);
        }

        private static string TruncateText(string text, int maxChars)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            if (text.Length <= maxChars)
                return text;

            return text.Substring(0, maxChars);
        }
    }
}

