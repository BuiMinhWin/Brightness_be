using AIOCRService.Application.DTOs;
using AIOCRService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AIOCRService.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OCRController : ControllerBase
    {
        private readonly IOCRService _ocrService;
        private readonly ILogger<OCRController> _logger;

        public OCRController(IOCRService ocrService, ILogger<OCRController> logger)
        {
            _ocrService = ocrService;
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
    }

    public class AIProcessRequest
    {
        public string Prompt { get; set; } = string.Empty;
    }
}

