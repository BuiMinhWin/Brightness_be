using System.Text.Json.Serialization;

namespace BillingAPI.Infrastructure.PaymentGateways.Dtos
{
    public class PayOsResponseDto
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("desc")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public PayOsDataDto? Data { get; set; }

        [JsonPropertyName("signature")]
        public string? Signature { get; set; }
    }
}
