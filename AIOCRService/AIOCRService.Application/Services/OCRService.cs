using AIOCRService.Application.DTOs;
using AIOCRService.Application.Interfaces;
using AIOCRService.Domain.Entities;
using System.Text.Json;

namespace AIOCRService.Application.Services
{
    public class OCRService : IOCRService
    {
        private readonly IOCRRepository _repository;
        private readonly IExternalOCRService _externalOCRService;
        private readonly IExternalAIService _externalAIService;

        public OCRService(
            IOCRRepository repository,
            IExternalOCRService externalOCRService,
            IExternalAIService externalAIService)
        {
            _repository = repository;
            _externalOCRService = externalOCRService;
            _externalAIService = externalAIService;
        }

        public async Task<OCRRequestDTO> CreateOCRRequestAsync(CreateOCRRequestDTO request)
        {
            var ocrRequest = new OCRRequest
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                ImageBase64 = request.ImageBase64,
                FileName = request.FileName,
                ContentType = request.ContentType,
                FileSize = request.FileSize,
                Type = request.Type,
                Status = "pending",
                CreatedAt = DateTime.UtcNow
            };

            var createdRequest = await _repository.CreateAsync(ocrRequest);
            return MapToDTO(createdRequest);
        }

        public async Task<OCRResultDTO> ProcessImageAsync(Guid requestId)
        {
            var request = await _repository.GetRequestByIdAsync(requestId);
            if (request == null)
                throw new ArgumentException("OCR request not found");

            try
            {
                request.Status = "processing";
                await _repository.UpdateAsync(request);

                // Extract text using external OCR service
                var extractedText = await _externalOCRService.ExtractTextFromImageAsync(
                    request.ImageBase64, request.ContentType);

                var result = new OCRResult
                {
                    Id = Guid.NewGuid(),
                    OCRRequestId = requestId,
                    ExtractedText = extractedText,
                    Confidence = "high", // This would come from the OCR service
                    Language = "vi", // Vietnamese by default, could be detected
                    CreatedAt = DateTime.UtcNow
                };

                var createdResult = await _repository.CreateResultAsync(result);

                // Update request status
                request.Status = "completed";
                request.ProcessedAt = DateTime.UtcNow;
                await _repository.UpdateAsync(request);

                return MapToDTO(createdResult);
            }
            catch (Exception ex)
            {
                request.Status = "failed";
                request.ErrorMessage = ex.Message;
                await _repository.UpdateAsync(request);
                throw;
            }
        }

        public async Task<ProcessedDataDTO> ProcessWithAIAsync(Guid resultId, string aiPrompt)
        {
            var result = await _repository.GetResultByIdAsync(resultId);
            if (result == null)
                throw new ArgumentException("OCR result not found");

            // Process with AI based on the data type - only one call
            var dataType = DetermineDataType((OCRType)result.OCRRequest.Type);
            var aiResponse = await _externalAIService.ProcessTextWithAIAsync(
                result.ExtractedText, aiPrompt, dataType);

            // Parse the structured JSON response
            var parsedResponse = ParseAIResponse(aiResponse);

            var processedData = new ProcessedData
            {
                Id = Guid.NewGuid(),
                OCRResultId = resultId,
                UserId = result.OCRRequest.UserId,
                DataType = dataType,
                ProcessedContent = aiResponse,
                Summary = parsedResponse.Summary,
                Insights = parsedResponse.Insights,
                CreatedAt = DateTime.UtcNow
            };

            var createdData = await _repository.CreateProcessedDataAsync(processedData);
            return MapToDTO(createdData);
        }

        public async Task<OCRRequestDTO> GetOCRRequestAsync(Guid requestId)
        {
            var request = await _repository.GetRequestByIdAsync(requestId);
            if (request == null)
                throw new ArgumentException("OCR request not found");

            return MapToDTO(request);
        }

        public async Task<OCRResultDTO> GetOCRResultAsync(Guid resultId)
        {
            var result = await _repository.GetResultByIdAsync(resultId);
            if (result == null)
                throw new ArgumentException("OCR result not found");

            return MapToDTO(result);
        }

        public async Task<ProcessedDataDTO> GetProcessedDataAsync(Guid processedDataId)
        {
            var data = await _repository.GetProcessedDataByIdAsync(processedDataId);
            if (data == null)
                throw new ArgumentException("Processed data not found");

            return MapToDTO(data);
        }

        public async Task<IEnumerable<OCRRequestDTO>> GetUserOCRRequestsAsync(Guid userId)
        {
            var requests = await _repository.GetByUserIdAsync(userId);
            return requests.Select(MapToDTO);
        }

        private static string DetermineDataType(OCRType type)
        {
            return type switch
            {
                OCRType.FoodLabel => "nutrition",
                OCRType.MedicalDocument => "medical",
                OCRType.Prescription => "medication",
                OCRType.LabResult => "lab_value",
                _ => "general"
            };
        }

        private static (string Summary, string Insights) ParseAIResponse(string aiResponse)
        {
            try
            {
                // Try to parse as JSON
                var jsonResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(aiResponse);
                
                if (jsonResponse != null)
                {
                    string summary = jsonResponse.summary?.ToString() ?? "Không có tóm tắt";
                    string insights = jsonResponse.recommendations?.ToString() ?? 
                                    jsonResponse.insights?.ToString() ?? 
                                    "Không có khuyến nghị";
                    
                    return (summary, insights);
                }
            }
            catch
            {
                // If JSON parsing fails, use the response as is
            }

            // Fallback: use the response as summary and create generic insights
            return (aiResponse, "Dựa trên phân tích trên, hãy tham khảo ý kiến chuyên gia để được tư vấn chi tiết hơn.");
        }

        private static OCRRequestDTO MapToDTO(OCRRequest request)
        {
            return new OCRRequestDTO
            {
                Id = request.Id,
                UserId = request.UserId,
                FileName = request.FileName,
                ContentType = request.ContentType,
                FileSize = request.FileSize,
                Type = request.Type,
                Status = request.Status,
                CreatedAt = request.CreatedAt,
                ProcessedAt = request.ProcessedAt,
                ErrorMessage = request.ErrorMessage
            };
        }

        private static OCRResultDTO MapToDTO(OCRResult result)
        {
            return new OCRResultDTO
            {
                Id = result.Id,
                OCRRequestId = result.OCRRequestId,
                ExtractedText = result.ExtractedText,
                StructuredData = result.StructuredData,
                Confidence = result.Confidence,
                Language = result.Language,
                CreatedAt = result.CreatedAt
            };
        }

        private static ProcessedDataDTO MapToDTO(ProcessedData data)
        {
            return new ProcessedDataDTO
            {
                Id = data.Id,
                OCRResultId = data.OCRResultId,
                UserId = data.UserId,
                DataType = data.DataType,
                ProcessedContent = data.ProcessedContent,
                Summary = data.Summary,
                Insights = data.Insights,
                CreatedAt = data.CreatedAt
            };
        }
    }
}

