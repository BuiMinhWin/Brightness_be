using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BillingAPI.Application.Requests
{
    public class CreateInvoiceRequest
    {
        [JsonPropertyName("userId")]
        public Guid? UserExternalId { get; set; }

        [Required]
        public Guid SubscriptionPlanId { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }

        [MaxLength(1024)]
        public string? Notes { get; set; }
    }
}
