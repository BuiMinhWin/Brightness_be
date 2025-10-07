using System.ComponentModel.DataAnnotations;

namespace AIOCRService.Domain.Entities
{
    public class OCRRequest
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string ImageBase64 { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public OCRType Type { get; set; }
        public string Status { get; set; } = "pending"; // pending, processing, completed, failed
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public enum OCRType
    {
        FoodLabel = 1,
        MedicalDocument = 2,
        Prescription = 3,
        LabResult = 4,
        General = 5
    }
}

