using System.ComponentModel.DataAnnotations;

namespace AIOCRService.Domain.Entities
{
    public class OCRResult
    {
        public Guid Id { get; set; }
        public Guid OCRRequestId { get; set; }
        public string ExtractedText { get; set; } = string.Empty;
        public string StructuredData { get; set; } = string.Empty; // JSON format
        public string Confidence { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        
        // Navigation property
        public OCRRequest OCRRequest { get; set; } = null!;
    }
}

