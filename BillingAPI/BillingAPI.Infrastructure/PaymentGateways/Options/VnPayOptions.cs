namespace BillingAPI.Infrastructure.PaymentGateways.Options
{
    public class VnPayOptions
    {
        public const string SectionName = "PaymentGateways:VnPay";

        public string TmnCode { get; set; } = string.Empty;
        public string HashSecret { get; set; } = string.Empty;
        public string PaymentUrl { get; set; } = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
        public string Version { get; set; } = "2.1.0";
        public string Command { get; set; } = "pay";
        public string Locale { get; set; } = "vn";
        public string CurrencyCode { get; set; } = "VND";
        public string ReturnUrl { get; set; } = string.Empty;
    }
}
