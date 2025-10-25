using AIOCRService.Application.DTOs;
using AIOCRService.Application.Interfaces;
using AIOCRService.Application.Helpers;
using AIOCRService.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace AIOCRService.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OCRController : ControllerBase
    {
        private readonly IOCRService _ocrService;
        private readonly IUserHealthDataService _healthDataService;
        private readonly IPersonalCoachApiService _personalCoachApiService;
        private readonly ILogger<OCRController> _logger;

        public OCRController(
            IOCRService ocrService, 
            IUserHealthDataService healthDataService,
            IPersonalCoachApiService personalCoachApiService,
            ILogger<OCRController> logger)
        {
            _ocrService = ocrService;
            _healthDataService = healthDataService;
            _personalCoachApiService = personalCoachApiService;
            _logger = logger;
        }

        /// <summary>
        /// Upload image for OCR processing
        /// </summary>
        [HttpPost("upload")]
        public async Task<ActionResult<OCRRequestDTO>> UploadImage([FromBody] CreateOCRRequestDTO request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.ImageBase64))
                {
                    return BadRequest("Image data is required");
                }

                var result = await _ocrService.CreateOCRRequestAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image for OCR");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Process uploaded image with OCR
        /// </summary>
        [HttpPost("{requestId}/process")]
        public async Task<ActionResult<OCRResultDTO>> ProcessImage(Guid requestId)
        {
            try
            {
                var result = await _ocrService.ProcessImageAsync(requestId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing image with OCR");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Process OCR result with AI for insights
        /// </summary>
        [HttpPost("{resultId}/ai-process")]
        public async Task<ActionResult<ProcessedDataDTO>> ProcessWithAI(
            Guid resultId, 
            [FromBody] AIProcessRequest request)
        {
            try
            {
                var result = await _ocrService.ProcessWithAIAsync(resultId, request.Prompt);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing with AI");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Process image directly with AI Vision (skip OCR step) - RECOMMENDED for InBody reports
        /// </summary>
        [HttpPost("requests/{requestId}/ai-vision-process")]
        public async Task<ActionResult<ProcessedDataDTO>> ProcessImageDirectlyWithAI(
            Guid requestId,
            [FromBody] AIProcessRequest request)
        {
            try
            {
                var result = await _ocrService.ProcessImageDirectlyWithAIAsync(requestId, request.Prompt);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing image directly with AI Vision");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Upload and process image directly with AI Vision (one-step) - BEST for InBody reports
        /// </summary>
        [HttpPost("ai-vision-upload")]
        public async Task<ActionResult<ProcessedDataDTO>> UploadAndProcessWithAIVision(
            IFormFile file,
            [FromForm] AIVisionUploadRequest request)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No image file provided");
                }

                // Check if user has sufficient credits for OCR processing
                var canAfford = await _personalCoachApiService.CanAffordOCRProcessingAsync(request.UserId);
                if (!canAfford)
                {
                    _logger.LogWarning("Insufficient credits for OCR processing for user {UserId}", request.UserId);
                    return StatusCode(402, new { 
                        success = false,
                        message = "Insufficient AI credits. OCR processing requires 5 credits. Please upgrade your subscription for more credits.",
                        code = "INSUFFICIENT_CREDITS"
                    });
                }

                // Convert file to base64
                string imageBase64;
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    var imageBytes = memoryStream.ToArray();
                    imageBase64 = Convert.ToBase64String(imageBytes);
                }

                // Create OCR request record for tracking
                var ocrRequest = await _ocrService.CreateOCRRequestAsync(new CreateOCRRequestDTO
                {
                    UserId = request.UserId,
                    ImageBase64 = imageBase64,
                    ContentType = file.ContentType,
                    FileName = file.FileName,
                    Type = OCRType.LabResult
                });

                // Process directly with AI Vision
                var result = await _ocrService.ProcessImageDirectlyWithAIAsync(ocrRequest.Id, request.Prompt);
                
                // Deduct credits after successful processing
                await _personalCoachApiService.DeductOCRProcessingCreditAsync(request.UserId);
                
                return Ok(result);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Insufficient credits"))
            {
                _logger.LogWarning(ex, "Insufficient credits for OCR processing for user {UserId}", request.UserId);
                return StatusCode(402, new { 
                    success = false,
                    message = "Insufficient AI credits for OCR processing. Please upgrade your subscription.",
                    code = "INSUFFICIENT_CREDITS"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading and processing with AI Vision");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get OCR request details
        /// </summary>
        [HttpGet("requests/{requestId}")]
        public async Task<ActionResult<OCRRequestDTO>> GetOCRRequest(Guid requestId)
        {
            try
            {
                var result = await _ocrService.GetOCRRequestAsync(requestId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting OCR request");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get OCR result details
        /// </summary>
        [HttpGet("results/{resultId}")]
        public async Task<ActionResult<OCRResultDTO>> GetOCRResult(Guid resultId)
        {
            try
            {
                var result = await _ocrService.GetOCRResultAsync(resultId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting OCR result");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get processed data details
        /// </summary>
        [HttpGet("processed/{processedDataId}")]
        public async Task<ActionResult<ProcessedDataDTO>> GetProcessedData(Guid processedDataId)
        {
            try
            {
                var result = await _ocrService.GetProcessedDataAsync(processedDataId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting processed data");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get user's OCR requests
        /// </summary>
        [HttpGet("users/{userId}/requests")]
        public async Task<ActionResult<IEnumerable<OCRRequestDTO>>> GetUserOCRRequests(Guid userId)
        {
            try
            {
                var result = await _ocrService.GetUserOCRRequestsAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user OCR requests");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get comprehensive health profile for a user (aggregates all health data)
        /// </summary>
        [HttpGet("users/{userId}/health-profile")]
        public async Task<ActionResult<UserHealthProfile>> GetUserHealthProfile(Guid userId)
        {
            try
            {
                var profile = await _healthDataService.GetUserHealthProfileAsync(userId);
                return Ok(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user health profile");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get user's latest InBody data with extracted metrics (easy for charts/graphs)
        /// </summary>
        [HttpGet("users/{userId}/latest-inbody")]
        public async Task<ActionResult<ProcessedDataWithMetrics>> GetLatestInBody(Guid userId)
        {
            try
            {
                var allData = await _ocrService.GetProcessedDataHistoryAsync(userId, "lab_result");
                var latest = allData.FirstOrDefault();
                
                if (latest == null)
                    return NotFound("No InBody data found for this user");

                // Parse and extract metrics from processedContent
                var enhanced = ParseToEnhancedDTO(latest);

                return Ok(enhanced);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting latest InBody data");
                return StatusCode(500, "Internal server error");
            }
        }

        private static ProcessedDataWithMetrics ParseToEnhancedDTO(ProcessedDataDTO dto)
        {
            var enhanced = new ProcessedDataWithMetrics
            {
                Id = dto.Id,
                OCRResultId = dto.OCRResultId,
                UserId = dto.UserId,
                DataType = dto.DataType,
                ProcessedContent = dto.ProcessedContent,
                Summary = dto.Summary,
                Insights = dto.Insights,
                IsLatest = dto.IsLatest,
                CreatedAt = dto.CreatedAt
            };

            // Extract metrics based on data type
            if (!string.IsNullOrEmpty(dto.ProcessedContent))
            {
                try
                {
                    var cleanedJson = AIResponseParser.CleanMarkdownCodeBlocks(dto.ProcessedContent);
                    var json = Newtonsoft.Json.Linq.JObject.Parse(cleanedJson);

                    if (dto.DataType == "lab_result")
                    {
                        enhanced.InBodyData = new InBodyMetrics
                        {
                            Weight = json["weight"]?.ToObject<decimal?>(),
                            BMI = json["bmi"]?.ToObject<decimal?>(),
                            BodyFatPercent = ParsePercentage(json["bodyFat"]?.ToString()),
                            MuscleMass = json["muscle"]?.ToObject<decimal?>(),
                            BMR = ParseInt(json["bmr"]?.ToString()),
                            VisceralFat = json["visceralFat"]?.ToObject<decimal?>(),
                            TotalBodyWater = json["totalBodyWater"]?.ToObject<decimal?>(),
                            Assessment = json["assessment"]?.ToString() ?? ""
                        };
                    }
                }
                catch
                {
                    // Metrics will be null if parsing fails
                }
            }

            return enhanced;
        }

        private static decimal? ParsePercentage(string? value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            var cleaned = value.Replace("%", "").Trim();
            return decimal.TryParse(cleaned, out var result) ? result : null;
        }

        private static int? ParseInt(string? value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            var cleaned = value.Replace("kcal", "").Replace("cal", "").Trim();
            return int.TryParse(cleaned, out var result) ? result : null;
        }

        /// <summary>
        /// Get AI-ready context summary for a user (for schedule generation, chat, etc.)
        /// </summary>
        [HttpGet("users/{userId}/ai-context")]
        public async Task<ActionResult<string>> GetUserAIContext(Guid userId)
        {
            try
            {
                var context = await _healthDataService.BuildUserContextForAIAsync(userId);
                return Ok(new { userId, context });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building user AI context");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get user data by specific type (latest only)
        /// </summary>
        [HttpGet("users/{userId}/data/{dataType}")]
        public async Task<ActionResult<IEnumerable<ProcessedDataDTO>>> GetUserDataByType(
            Guid userId, 
            string dataType)
        {
            try
            {
                var data = await _healthDataService.GetUserDataByTypeAsync(userId, dataType);
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user data by type");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get full history of user data by type (including old versions)
        /// </summary>
        [HttpGet("users/{userId}/data/{dataType}/history")]
        public async Task<ActionResult<IEnumerable<ProcessedDataDTO>>> GetUserDataHistory(
            Guid userId,
            string dataType)
        {
            try
            {
                var history = await _ocrService.GetProcessedDataHistoryAsync(userId, dataType);
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user data history");
                return StatusCode(500, "Internal server error");
            }
        }

    }

    public class AIProcessRequest
    {
        public string Prompt { get; set; } = string.Empty;
    }
}


