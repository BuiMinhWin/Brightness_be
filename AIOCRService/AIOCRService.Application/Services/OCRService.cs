using AIOCRService.Application.DTOs;
using AIOCRService.Application.Interfaces;
using AIOCRService.Application.Helpers;
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
            return createdRequest.ToDTO();
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
            var parsedResponse = AIResponseParser.ParseAIResponse(aiResponse);

            // Mark all previous versions as not latest
            await _repository.MarkPreviousVersionsAsOldAsync(resultId, result.OCRRequest.UserId, dataType);

            var processedData = new ProcessedData
            {
                Id = Guid.NewGuid(),
                OCRResultId = resultId,
                UserId = result.OCRRequest.UserId,
                DataType = dataType,
                ProcessedContent = parsedResponse.CleanedJson,
                Summary = parsedResponse.Summary,
                Insights = parsedResponse.Insights,
                IsLatest = true,
                CreatedAt = DateTime.UtcNow
            };

            var createdData = await _repository.CreateProcessedDataAsync(processedData);
            return MapToDTO(createdData);
        }

        public async Task<ProcessedDataDTO> ProcessImageDirectlyWithAIAsync(Guid requestId, string aiPrompt)
        {
            var request = await _repository.GetRequestByIdAsync(requestId);
            if (request == null)
                throw new ArgumentException("OCR request not found");

            // Process image directly with AI (no OCR step)
            var dataType = DetermineDataType((OCRType)request.Type);
            var aiResponse = await _externalAIService.ProcessImageDirectlyAsync(
                request.ImageBase64, aiPrompt, dataType);

            // Parse the structured JSON response
            var parsedResponse = AIResponseParser.ParseAIResponse(aiResponse);

            // Mark all previous versions for this request/dataType as not latest
            await _repository.MarkPreviousVersionsAsOldAsync(null, request.UserId, dataType);

            var processedData = new ProcessedData
            {
                Id = Guid.NewGuid(),
                OCRResultId = null, // No OCR result since we processed image directly
                UserId = request.UserId,
                DataType = dataType,
                ProcessedContent = parsedResponse.CleanedJson,
                Summary = parsedResponse.Summary,
                Insights = parsedResponse.Insights,
                IsLatest = true,
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

        public async Task<IEnumerable<ProcessedDataDTO>> GetProcessedDataHistoryAsync(Guid userId, string dataType)
        {
            var history = await _repository.GetProcessedDataHistoryAsync(userId, dataType);
            return history.Select(MapToDTO);
        }

        private static string DetermineDataType(OCRType type) => type.ToDataType();


        private static OCRRequestDTO MapToDTO(OCRRequest request) => request.ToDTO();
        private static OCRResultDTO MapToDTO(OCRResult result) => result.ToDTO();
        private static ProcessedDataDTO MapToDTO(ProcessedData data) => data.ToDTO();
    }
}

