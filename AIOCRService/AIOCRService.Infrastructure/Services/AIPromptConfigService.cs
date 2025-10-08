namespace AIOCRService.Infrastructure.Services
{
    public static class AIPromptConfigService
    {
        public static string GetSystemPrompt(string dataType)
        {
            return dataType switch
            {
                "nutrition" or "food_label" => 
                    @"Phân tích nhãn dinh dưỡng, BẮT BUỘC trả về JSON với TẤT CẢ fields:
{
  ""summary"": ""Tóm tắt ngắn gọn"",
  ""calories"": 250,
  ""protein"": ""15g"",
  ""carbs"": ""30g"",
  ""fat"": ""10g"",
  ""fiber"": ""5g"",
  ""sugar"": ""8g"",
  ""sodium"": ""500mg"",
  ""healthScore"": 7,
  ""keyNutrients"": [""Vitamin C"", ""Calcium""],
  ""recommendations"": [""Khuyến nghị 1"", ""Khuyến nghị 2""]
}
Trích xuất CHÍNH XÁC từ nhãn, không bịa số liệu.",
                
                "medical" or "medical_document" => 
                    @"Phân tích tài liệu y tế, trả về JSON:
{""summary"":""Tóm tắt"",""keyFindings"":[""phát hiện""],""conditions"":[""tình trạng""],""recommendations"":[""khuyến nghị""],""urgency"":""thấp/trung bình/cao""}",
                
                "medication" or "prescription" => 
                    @"Phân tích đơn thuốc, trả về JSON:
{""summary"":""Tóm tắt"",""medications"":[{""name"":""tên thuốc"",""dosage"":""liều lượng"",""frequency"":""tần suất""}],""warnings"":[""cảnh báo""],""sideEffects"":[""tác dụng phụ""]}",
                
                "lab_value" or "lab_result" => 
                    @"Phân tích InBody, BẮT BUỘC trả về JSON với TẤT CẢ fields sau (dùng số thực tế từ dữ liệu):
{
  ""summary"": ""Tóm tắt ngắn gọn"",
  ""weight"": 163.3,
  ""bmi"": 24.1,
  ""bodyFat"": ""8.2%"",
  ""muscle"": 87.5,
  ""bmr"": ""1838 kcal"",
  ""visceralFat"": 11.3,
  ""totalBodyWater"": 109.6,
  ""assessment"": ""Đánh giá chi tiết về thể trạng"",
  ""recommendations"": [
    ""Khuyến nghị 1 cụ thể"",
    ""Khuyến nghị 2 cụ thể"",
    ""Khuyến nghị 3 cụ thể""
  ]
}
QUAN TRỌNG: Trích xuất CHÍNH XÁC các số liệu từ dữ liệu, không bịa thêm.",
                
                "injury_history" => 
                    @"Phân tích chấn thương, BẮT BUỘC trả về JSON đầy đủ:
{
  ""summary"": ""Tóm tắt"",
  ""injuries"": [
    {""type"":""ACL tear"",""location"":""Knee"",""date"":""2023-05"",""severity"":""High""}
  ],
  ""restrictions"": [""Tránh squat nặng"",""Tránh chạy""],
  ""recovery"": [""Tập phục hồi với PT"",""Bơi lội""]
}",
                
                "fitness_assessment" => 
                    @"Phân tích thể lực, BẮT BUỘC JSON đầy đủ:
{
  ""summary"": ""Tóm tắt"",
  ""tests"": [{""name"":""VO2 Max"",""result"":""45 ml/kg/min"",""rating"":""Tốt""}],
  ""strengths"": [""Điểm mạnh""],
  ""weaknesses"": [""Cần cải thiện""],
  ""overallLevel"": ""Intermediate"",
  ""recommendations"": [""Khuyến nghị""]
}",
                
                "meal_plan" => 
                    @"Phân tích thực đơn, BẮT BUỘC JSON đầy đủ:
{
  ""summary"": ""Tóm tắt"",
  ""totalCalories"": 2000,
  ""macros"": {""protein"":""150g"",""carbs"":""200g"",""fat"":""67g""},
  ""meals"": [{""name"":""Breakfast"",""foods"":[""Eggs"",""Oats""]}],
  ""analysis"": [""Cân bằng macro""],
  ""recommendations"": [""Khuyến nghị""]
}",
                
                "workout_log" => 
                    @"Phân tích workout log, BẮT BUỘC JSON đầy đủ:
{
  ""summary"": ""Tóm tắt"",
  ""exercises"": [{""name"":""Bench Press"",""sets"":""4"",""reps"":""10"",""weight"":""80kg""}],
  ""volume"": ""3200kg"",
  ""analysis"": [""Tăng volume 10%""],
  ""progress"": [""Tiến bộ tốt""],
  ""recommendations"": [""Khuyến nghị""]
}",
                
                "health_questionnaire" => 
                    @"Phân tích bảng câu hỏi sức khỏe, trả về JSON:
{""summary"":""Tóm tắt"",""healthConditions"":[""tình trạng""],""medications"":[""thuốc đang dùng""],""allergies"":[""dị ứng""],""lifestyle"":{""sleep"":""giờ"",""stress"":""mức độ"",""activity"":""mức độ""},""riskFactors"":[""yếu tố rủi ro""]}",
                
                "progress_photo" => 
                    @"Phân tích ảnh tiến triển cơ thể, trả về JSON:
{""summary"":""Tóm tắt"",""observations"":[""quan sát""],""changes"":[""thay đổi nhìn thấy""],""bodyAreas"":{""chest"":""nhận xét"",""arms"":""nhận xét"",""core"":""nhận xét"",""legs"":""nhận xét""},""recommendations"":[""khuyến nghị""]}",
                
                "allergy_test" => 
                    @"Phân tích kết quả test dị ứng, trả về JSON:
{""summary"":""Tóm tắt"",""allergies"":[{""allergen"":""chất gây dị ứng"",""severity"":""mức độ"",""reaction"":""phản ứng""}],""restrictions"":[""thực phẩm cần tránh""],""alternatives"":[""thay thế""],""warnings"":[""cảnh báo""]}",
                
                "supplement_label" => 
                    @"Phân tích nhãn thực phẩm bổ sung, trả về JSON:
{""summary"":""Tóm tắt"",""ingredients"":[{""name"":""thành phần"",""amount"":""hàm lượng"",""dailyValue"":""% nhu cầu""}],""benefits"":[""lợi ích""],""warnings"":[""cảnh báo""],""timing"":""thời điểm uống""}",
                
                _ => 
                    @"Phân tích tài liệu, trả về JSON:
{""summary"":""Tóm tắt"",""keyPoints"":[""điểm chính""],""insights"":[""insight""],""recommendations"":[""khuyến nghị""]}"
            };
        }

        public static string GetUserPrompt(string dataType)
        {
            return dataType switch
            {
                "nutrition" or "food_label" => "Phân tích nhãn dinh dưỡng. Trả về JSON:",
                "medical" or "medical_document" => "Phân tích tài liệu y tế. Trả về JSON:",
                "medication" or "prescription" => "Phân tích đơn thuốc. Trả về JSON:",
                "lab_value" or "lab_result" => "Trích xuất số liệu InBody/xét nghiệm. Trả về JSON:",
                "injury_history" => "Phân tích lịch sử chấn thương. Trả về JSON:",
                "fitness_assessment" => "Phân tích đánh giá thể lực. Trả về JSON:",
                "meal_plan" => "Phân tích thực đơn. Trả về JSON:",
                "workout_log" => "Phân tích nhật ký tập luyện. Trả về JSON:",
                "health_questionnaire" => "Phân tích bảng câu hỏi sức khỏe. Trả về JSON:",
                "progress_photo" => "Phân tích ảnh tiến triển cơ thể. Trả về JSON:",
                "allergy_test" => "Phân tích test dị ứng. Trả về JSON:",
                "supplement_label" => "Phân tích nhãn thực phẩm bổ sung. Trả về JSON:",
                _ => "Phân tích dữ liệu. Trả về JSON:"
            };
        }

        public static Dictionary<string, int> GetTokenLimits()
        {
            return new Dictionary<string, int>
            {
                { "maxOutputTokens", 8000 },
                { "maxInputChars", 1000 }
            };
        }

        public static Dictionary<string, object> GetGenerationConfig()
        {
            return new Dictionary<string, object>
            {
                { "temperature", 0.7 },
                { "maxOutputTokens", 8000 },
                { "topP", 1.0 },
                { "topK", 64 }
            };
        }
    }
}

