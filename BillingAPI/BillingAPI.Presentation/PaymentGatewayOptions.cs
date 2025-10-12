namespace BillingAPI.Presentation
{
    public class PaymentGatewayOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public string? Endpoint { get; set; }
    }
}
