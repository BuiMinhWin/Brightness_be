using AIOCRService.Domain.Entities;

namespace AIOCRService.Application.DTOs
{
    public class OCRRequestDTO
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public OCRType Type { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class CreateOCRRequestDTO
    {
        public Guid UserId { get; set; }
        public string ImageBase64 { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public OCRType Type { get; set; }
    }

    public class OCRResultDTO
    {
        public Guid Id { get; set; }
        public Guid OCRRequestId { get; set; }
        public string ExtractedText { get; set; } = string.Empty;
        public string StructuredData { get; set; } = string.Empty;
        public string Confidence { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class ProcessedDataDTO
    {
        public Guid Id { get; set; }
        public Guid OCRResultId { get; set; }
        public Guid UserId { get; set; }
        public string DataType { get; set; } = string.Empty;
        public string ProcessedContent { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string Insights { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}

