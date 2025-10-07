namespace BillingAPI.Infrastructure.PaymentGateways.Options
{
    public class PayOsOptions
    {
        public const string SectionName = "PaymentGateways:PayOs";

        public string ClientId { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string ChecksumKey { get; set; } = string.Empty;
        public string Endpoint { get; set; } = "https://api-merchant.payos.vn/v2";
        public string ReturnUrl { get; set; } = string.Empty;
        public string CancelUrl { get; set; } = string.Empty;
    }
}
