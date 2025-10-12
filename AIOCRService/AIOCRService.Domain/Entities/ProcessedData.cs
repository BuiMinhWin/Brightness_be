using System.ComponentModel.DataAnnotations;

namespace AIOCRService.Domain.Entities
{
    public class ProcessedData
    {
        public Guid Id { get; set; }
        public Guid? OCRResultId { get; set; } // Nullable for direct AI vision processing
        public Guid UserId { get; set; }
        public string DataType { get; set; } = string.Empty; // nutrition, medication, lab_value, etc.
        public string ProcessedContent { get; set; } = string.Empty; // AI processed and structured data
        public string Summary { get; set; } = string.Empty; // AI generated summary
        public string Insights { get; set; } = string.Empty; // AI insights and recommendations
        public bool IsLatest { get; set; } = true; // Track latest version for each OCRRequest or DataType
        public DateTime CreatedAt { get; set; }
        
        // Navigation property
        public OCRResult? OCRResult { get; set; }
    }
}

