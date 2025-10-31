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
                    @"Phân tích tài liệu y tế - Trích xuất TẤT CẢ thông tin, BẮT BUỘC JSON đầy đủ:
{
  ""summary"": ""Tóm tắt báo cáo y tế"",
  ""patientInfo"": {
    ""name"": ""Tên bệnh nhân"",
    ""age"": 30,
    ""gender"": ""male/female"",
    ""dateOfBirth"": ""1995-01-15""
  },
  ""vitalSigns"": {
    ""bloodPressure"": ""120/80"",
    ""heartRate"": 72,
    ""temperature"": 36.5,
    ""respiratoryRate"": 16,
    ""oxygenSaturation"": 98
  },
  ""physicalMeasurements"": {
    ""height"": 175,
    ""weight"": 70,
    ""bmi"": 22.9,
    ""waist"": 80,
    ""chest"": 95,
    ""hips"": 95
  },
  ""diagnosis"": [""Chẩn đoán 1"", ""Chẩn đoán 2""],
  ""symptoms"": [""Triệu chứng""],
  ""testResults"": [
    {""test"": ""Tên xét nghiệm"", ""result"": ""Kết quả"", ""normalRange"": ""Giới hạn bình thường"", ""status"": ""normal/abnormal""}
  ],
  ""medications"": [
    {""name"": ""Tên thuốc"", ""dosage"": ""Liều lượng"", ""frequency"": ""Tần suất""}
  ],
  ""medicalHistory"": [""Tiền sử bệnh""],
  ""allergies"": [""Dị ứng""],
  ""chronicConditions"": [""Bệnh mãn tính""],
  ""surgeries"": [
    {""procedure"": ""Phẫu thuật"", ""date"": ""2020-05"", ""notes"": ""Ghi chú""}
  ],
  ""recommendations"": [""Khuyến nghị điều trị""],
  ""restrictions"": [""Hạn chế/kiêng kị""],
  ""followUp"": ""Lịch tái khám"",
  ""urgency"": ""low/medium/high""
}

HƯỚNG DẪN: Trích xuất TẤT CẢ thông tin có sẵn, không bỏ sót. Nếu field nào không có dữ liệu thì bỏ qua.",
                
                "medication" or "prescription" => 
                    @"Phân tích đơn thuốc - Trích xuất TẤT CẢ thông tin, BẮT BUỘC JSON đầy đủ:
{
  ""summary"": ""Tóm tắt đơn thuốc"",
  ""prescriptionDate"": ""2025-10-12"",
  ""doctor"": {
    ""name"": ""Tên bác sĩ"",
    ""specialty"": ""Chuyên khoa"",
    ""clinic"": ""Phòng khám""
  },
  ""patient"": {
    ""name"": ""Tên bệnh nhân"",
    ""age"": 30,
    ""diagnosis"": ""Chẩn đoán""
  },
  ""medications"": [
    {
      ""name"": ""Tên thuốc"",
      ""genericName"": ""Tên hoạt chất"",
      ""dosage"": ""500mg"",
      ""frequency"": ""2 lần/ngày"",
      ""duration"": ""7 ngày"",
      ""route"": ""oral/injection/topical"",
      ""instructions"": ""Uống sau ăn"",
      ""quantity"": ""14 viên""
    }
  ],
  ""warnings"": [""Không uống với rượu"", ""Tránh lái xe""],
  ""sideEffects"": [""Buồn ngủ"", ""Chóng mặt""],
  ""contraindications"": [""Chống chỉ định""],
  ""interactions"": [""Tương tác thuốc""],
  ""storageInstructions"": ""Bảo quản nơi khô ráo"",
  ""refillsAllowed"": 2,
  ""expiryDate"": ""2025-12-31"",
  ""recommendations"": [""Khuyến nghị điều trị""]
}

HƯỚNG DẪN: Trích xuất CHÍNH XÁC tên thuốc, liều lượng, tần suất. Bao gồm TẤT CẢ cảnh báo an toàn.",
                
                "lab_value" or "lab_result" => 
                    @"Phân tích InBody/Lab Report - Trích xuất TẤT CẢ dữ liệu có sẵn, BẮT BUỘC trả về JSON đầy đủ:
{
  ""summary"": ""Tóm tắt ngắn gọn về thể trạng"",
  
  // Basic Measurements (REQUIRED)
  ""weight"": 163.3,
  ""weightUnit"": ""lbs"",
  ""height"": 175,
  ""heightUnit"": ""cm"",
  ""bmi"": 24.1,
  ""age"": 30,
  ""gender"": ""male"",
  
  // Body Composition (Extract ALL if available)
  ""bodyFat"": ""8.2%"",
  ""bodyFatMass"": 13.4,
  ""leanBodyMass"": 149.9,
  ""muscleMass"": 87.5,
  ""skeletalMuscleMass"": 87.5,
  ""boneMass"": 8.2,
  
  // Water & Protein
  ""totalBodyWater"": 109.6,
  ""intracellularWater"": 70.4,
  ""extracellularWater"": 39.2,
  ""ecwTbwRatio"": 0.357,
  ""protein"": 39.2,
  
  // Metabolic
  ""bmr"": ""1838 kcal"",
  ""visceralFat"": 11.3,
  ""metabolicAge"": 25,
  
  // Segmental Analysis (if available)
  ""rightArm"": {""muscle"": 7.8, ""fat"": 0.5},
  ""leftArm"": {""muscle"": 7.9, ""fat"": 0.5},
  ""trunk"": {""muscle"": 58.2, ""fat"": 3.9},
  ""rightLeg"": {""muscle"": 23.1, ""fat"": 1.2},
  ""leftLeg"": {""muscle"": 23.3, ""fat"": 1.2},
  
  // Blood Test Results (if applicable)
  ""cholesterol"": {""total"": 180, ""hdl"": 55, ""ldl"": 110, ""triglycerides"": 75},
  ""glucose"": 90,
  ""bloodPressure"": {""systolic"": 120, ""diastolic"": 80},
  ""heartRate"": 72,
  
  // Fitness Metrics (if available)
  ""vo2Max"": 45,
  ""bodyAge"": 25,
  ""fitnessScore"": 85,
  
  // Health Assessment
  ""assessment"": ""Đánh giá chi tiết về thể trạng, điểm mạnh, điểm yếu"",
  ""healthRisks"": [""Các rủi ro sức khỏe nếu có""],
  ""recommendations"": [
    ""Khuyến nghị 1 cụ thể về dinh dưỡng"",
    ""Khuyến nghị 2 về tập luyện"",
    ""Khuyến nghị 3 về lối sống"",
    ""Khuyến nghị 4 về theo dõi sức khỏe""
  ]
}

HƯỚNG DẪN:
1. Trích xuất TẤT CẢ số liệu có trong báo cáo, không bỏ sót
2. Nếu không có dữ liệu cho field nào, đặt null hoặc bỏ qua field đó
3. Đảm bảo số liệu CHÍNH XÁC 100%, không ước lượng hay bịa
4. Bao gồm cả đơn vị đo (lbs, kg, %, cm, etc)
5. Phân tích toàn diện để đưa ra khuyến nghị cá nhân hóa",
                
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
                    @"Phân tích đánh giá thể lực - Trích xuất TẤT CẢ dữ liệu, BẮT BUỘC JSON đầy đủ:
{
  ""summary"": ""Tóm tắt đánh giá thể lực tổng quan"",
  ""assessmentDate"": ""2025-10-12"",
  ""personalInfo"": {
    ""name"": ""Tên"",
    ""age"": 30,
    ""gender"": ""male/female"",
    ""height"": 175,
    ""weight"": 70
  },
  ""cardioTests"": {
    ""vo2Max"": 45,
    ""restingHeartRate"": 60,
    ""maxHeartRate"": 185,
    ""recoveryHeartRate"": 120,
    ""beepTest"": ""Level 12.5"",
    ""cooperTest"": ""2800m""
  },
  ""strengthTests"": {
    ""benchPress1RM"": ""80kg"",
    ""squat1RM"": ""120kg"",
    ""deadlift1RM"": ""150kg"",
    ""pushUps"": 50,
    ""pullUps"": 15,
    ""plankTime"": ""3 minutes""
  },
  ""flexibilityTests"": {
    ""sitAndReach"": ""25cm"",
    ""shoulderFlexibility"": ""Good"",
    ""hipMobility"": ""Excellent""
  },
  ""bodyComposition"": {
    ""bodyFat"": ""12%"",
    ""muscleMass"": ""65kg"",
    ""bmi"": 22.9
  },
  ""functionalTests"": {
    ""balance"": ""Excellent"",
    ""coordination"": ""Good"",
    ""agility"": ""Average"",
    ""power"": ""Good""
  },
  ""overallFitnessLevel"": ""intermediate"",
  ""fitnessScore"": 85,
  ""strengths"": [""Sức bền tim mạch tốt"", ""Sức mạnh cơ core""],
  ""weaknesses"": [""Cần cải thiện độ linh hoạt"", ""Sức mạnh chân yếu hơn""],
  ""healthRisks"": [""Rủi ro nếu có""],
  ""recommendations"": [
    ""Tăng cường tập sức mạnh chân"",
    ""Thêm yoga/stretching để cải thiện độ linh hoạt"",
    ""Duy trì cardio 3-4 lần/tuần""
  ],
  ""goals"": [""Mục tiêu đề xuất""]
}

HƯỚNG DẪN: Trích xuất TẤT CẢ số liệu kiểm tra thể lực, bao gồm cardio, strength, flexibility. Đánh giá mức độ tổng thể.",
                
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
                { "maxOutputTokens", 16384 }, // Increased for Gemini 2.5 Flash extended thinking
                { "maxInputChars", 1000 }
            };
        }

        public static Dictionary<string, object> GetGenerationConfig()
        {
            return new Dictionary<string, object>
            {
                { "temperature", 0.7 },
                { "maxOutputTokens", 16384 }, // Increased for Gemini 2.5 Flash extended thinking
                { "topP", 1.0 },
                { "topK", 64 }
            };
        }
    }
}

