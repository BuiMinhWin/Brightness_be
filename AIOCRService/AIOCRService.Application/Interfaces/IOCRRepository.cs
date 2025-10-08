using AIOCRService.Domain.Entities;

namespace AIOCRService.Application.Interfaces
{
    public interface IOCRRepository
    {
        Task<OCRRequest> CreateAsync(OCRRequest request);
        Task<OCRRequest?> GetRequestByIdAsync(Guid id);
        Task<OCRResult?> GetResultByIdAsync(Guid id);
        Task<ProcessedData?> GetProcessedDataByIdAsync(Guid id);
        Task<OCRResult> CreateResultAsync(OCRResult result);
        Task<ProcessedData> CreateProcessedDataAsync(ProcessedData data);
        Task<IEnumerable<OCRRequest>> GetByUserIdAsync(Guid userId);
        Task UpdateAsync(OCRRequest request);
    }
}

