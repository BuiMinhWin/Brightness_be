using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace AIOCRService.Application.Helpers
{
    public static class AIResponseParser
    {
        public static (string Summary, string Insights, string CleanedJson) ParseAIResponse(string aiResponse)
        {
            try
            {
                // Clean the response - remove markdown code blocks
                var cleanedResponse = CleanMarkdownCodeBlocks(aiResponse);

                // Try to parse as JSON
                var jsonResponse = JsonConvert.DeserializeObject<dynamic>(cleanedResponse);
                
                if (jsonResponse != null)
                {
                    string summary = ExtractSummary(jsonResponse);
                    string insights = ExtractInsights(jsonResponse);
                    
                    // Clean all string values in the JSON object
                    CleanJsonObject(jsonResponse);
                    
                    // Re-serialize to clean format
                    var prettyJson = JsonConvert.SerializeObject(jsonResponse, Formatting.Indented);
                    
                    return (summary, insights, prettyJson);
                }
            }
            catch
            {
                // If JSON parsing fails, try to extract JSON from text
                var extractedJson = ExtractJsonFromText(aiResponse);
                if (!string.IsNullOrEmpty(extractedJson))
                {
                    try
                    {
                        var jsonResponse = JsonConvert.DeserializeObject<dynamic>(extractedJson);
                        string summary = ExtractSummary(jsonResponse);
                        string insights = ExtractInsights(jsonResponse);
                        
                        // Clean all string values
                        CleanJsonObject(jsonResponse);
                        
                        // Re-serialize to clean format
                        var prettyJson = JsonConvert.SerializeObject(jsonResponse, Formatting.Indented);
                        
                        return (summary, insights, prettyJson);
                    }
                    catch
                    {
                        // Continue to fallback
                    }
                }
            }

            // Fallback: use the response as summary
            return (aiResponse, "Tham khảo ý kiến chuyên gia để được tư vấn chi tiết hơn.", aiResponse);
        }

        public static string CleanMarkdownCodeBlocks(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            // Remove ```json and ``` markers
            text = Regex.Replace(text, @"^```json\s*", "", RegexOptions.Multiline);
            text = Regex.Replace(text, @"^```\s*$", "", RegexOptions.Multiline);
            text = text.Trim();

            return text;
        }

        private static string ExtractJsonFromText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            // Try to find JSON object in the text
            var match = Regex.Match(text, @"\{.*\}", RegexOptions.Singleline);
            if (match.Success)
            {
                return match.Value;
            }

            return string.Empty;
        }

        private static string ExtractSummary(dynamic jsonResponse)
        {
            try
            {
                var jObject = jsonResponse as JObject;
                if (jObject != null)
                {
                    // Try multiple field name variations (English & Vietnamese)
                    var summaryFields = new[] { 
                        "summary", "Summary", "tóm_tắt", "tom_tat", 
                        "description", "Description", "mô_tả", "mo_ta"
                    };

                    foreach (var field in summaryFields)
                    {
                        var value = jObject[field];
                        if (value != null && value.Type == JTokenType.String)
                        {
                            return CleanText(value.ToString());
                        }
                    }

                    // If no summary field, use first string property
                    var firstStringProp = jObject.Properties()
                        .FirstOrDefault(p => p.Value.Type == JTokenType.String);
                    
                    if (firstStringProp != null)
                    {
                        return CleanText(firstStringProp.Value.ToString());
                    }
                }

                return "Không có tóm tắt";
            }
            catch
            {
                return "Không có tóm tắt";
            }
        }

        private static string ExtractInsights(dynamic jsonResponse)
        {
            try
            {
                var jObject = jsonResponse as JObject;
                if (jObject == null) return "Không có khuyến nghị";

                var insightParts = new List<string>();

                // Try multiple field names for recommendations (English & Vietnamese)
                var recommendationFields = new[] { 
                    "recommendations", "Recommendations", "khuyến_nghị", "khuyen_nghi",
                    "insights", "Insights", 
                    "advice", "Advice", "lời_khuyên", "loi_khuyen"
                };

                foreach (var field in recommendationFields)
                {
                    var value = jObject[field];
                    if (value != null)
                    {
                        if (value is JArray array)
                        {
                            var items = array.Select(x => CleanText(x.ToString())).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                            insightParts.AddRange(items);
                            break; // Found recommendations, stop looking
                        }
                        else if (value.Type == JTokenType.String)
                        {
                            var text = CleanText(value.ToString());
                            if (!string.IsNullOrWhiteSpace(text))
                            {
                                insightParts.Add(text);
                                break;
                            }
                        }
                    }
                }

                if (insightParts.Any())
                {
                    // Return as numbered list for readability
                    return string.Join("\n", insightParts.Select((text, index) => $"{index + 1}. {text}"));
                }

                return "Không có khuyến nghị";
            }
            catch
            {
                return "Không có khuyến nghị";
            }
        }

        private static void CleanJsonObject(dynamic jsonObject)
        {
            if (jsonObject == null) return;

            if (jsonObject is JObject jObj)
            {
                foreach (var property in jObj.Properties().ToList())
                {
                    if (property.Value.Type == JTokenType.String)
                    {
                        // Clean string values
                        var cleaned = CleanText(property.Value.ToString());
                        property.Value = cleaned;
                    }
                    else if (property.Value.Type == JTokenType.Array)
                    {
                        var array = property.Value as JArray;
                        if (array != null)
                        {
                            for (int i = 0; i < array.Count; i++)
                            {
                                if (array[i].Type == JTokenType.String)
                                {
                                    array[i] = CleanText(array[i].ToString());
                                }
                                else if (array[i].Type == JTokenType.Object)
                                {
                                    CleanJsonObject(array[i]);
                                }
                            }
                        }
                    }
                    else if (property.Value.Type == JTokenType.Object)
                    {
                        CleanJsonObject(property.Value);
                    }
                }
            }
        }

        private static string CleanText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            // Remove escaped newlines and extra whitespace
            return text
                .Replace("\\n", " ")
                .Replace("\n", " ")
                .Replace("  ", " ")
                .Trim();
        }

        public static string PrettifyJson(string json)
        {
            try
            {
                var obj = JsonConvert.DeserializeObject(json);
                return JsonConvert.SerializeObject(obj, Formatting.Indented);
            }
            catch
            {
                return json;
            }
        }

        public static T? ExtractMetrics<T>(string processedContent) where T : class
        {
            try
            {
                var json = CleanMarkdownCodeBlocks(processedContent);
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch
            {
                return null;
            }
        }
    }
}

