using AIOCRService.Application.DTOs;

namespace AIOCRService.Application.Interfaces
{
    public interface IOCRService
    {
        Task<OCRRequestDTO> CreateOCRRequestAsync(CreateOCRRequestDTO request);
        Task<OCRResultDTO> ProcessImageAsync(Guid requestId);
        Task<ProcessedDataDTO> ProcessWithAIAsync(Guid resultId, string aiPrompt);
        Task<OCRRequestDTO> GetOCRRequestAsync(Guid requestId);
        Task<OCRResultDTO> GetOCRResultAsync(Guid resultId);
        Task<ProcessedDataDTO> GetProcessedDataAsync(Guid processedDataId);
        Task<IEnumerable<OCRRequestDTO>> GetUserOCRRequestsAsync(Guid userId);
    }
}

