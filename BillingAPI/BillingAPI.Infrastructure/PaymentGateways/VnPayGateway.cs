using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BillingAPI.Application.Interfaces;
using BillingAPI.Application.Responses;
using BillingAPI.Domain.Entities;
using BillingAPI.Infrastructure.PaymentGateways.Options;
using Microsoft.Extensions.Options;

namespace BillingAPI.Infrastructure.PaymentGateways
{
    public class VnPayGateway(IOptions<VnPayOptions> options) : IVnPayGateway
    {
        private readonly VnPayOptions _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

        public Task<GatewayPaymentResult> CreatePaymentUrlAsync(Invoice invoice, Payment payment, string returnUrl, string? cancelUrl, string? clientIp)
        {
            var amount = Convert.ToInt64(Math.Round(payment.Amount * 100, MidpointRounding.AwayFromZero));
            var parameters = new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                ["vnp_Version"] = _options.Version,
                ["vnp_Command"] = _options.Command,
                ["vnp_TmnCode"] = _options.TmnCode,
                ["vnp_Amount"] = amount.ToString(CultureInfo.InvariantCulture),
                ["vnp_CurrCode"] = _options.CurrencyCode,
                ["vnp_TxnRef"] = payment.Id.ToString("N"),
                ["vnp_OrderInfo"] = $"Invoice {invoice.InvoiceCode}",
                ["vnp_OrderType"] = "billpayment",
                ["vnp_Locale"] = _options.Locale,
                ["vnp_ReturnUrl"] = string.IsNullOrWhiteSpace(returnUrl) ? _options.ReturnUrl : returnUrl,
                ["vnp_IpAddr"] = clientIp ?? "127.0.0.1",
                ["vnp_CreateDate"] = DateTime.UtcNow.ToString("yyyyMMddHHmmss"),
                ["vnp_ExpireDate"] = DateTime.UtcNow.AddMinutes(15).ToString("yyyyMMddHHmmss")
            };

            var query = BuildQuery(parameters);
            var secureHash = ComputeHash(parameters);
            var paymentUrl = $"{_options.PaymentUrl}?{query}&vnp_SecureHash={secureHash}";

            return Task.FromResult(new GatewayPaymentResult(paymentUrl, DateTime.UtcNow.AddMinutes(15), null));
        }

        public bool ValidateSignature(Dictionary<string, string> queryParameters)
        {
            if (!queryParameters.TryGetValue("vnp_SecureHash", out var secureHash))
            {
                return false;
            }

            var parameters = new SortedDictionary<string, string>(StringComparer.Ordinal);
            foreach (var pair in queryParameters)
            {
                if (!string.Equals(pair.Key, "vnp_SecureHash", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(pair.Key, "vnp_SecureHashType", StringComparison.OrdinalIgnoreCase))
                {
                    parameters[pair.Key] = pair.Value;
                }
            }

            var computedHash = ComputeHash(parameters);
            return string.Equals(computedHash, secureHash, StringComparison.OrdinalIgnoreCase);
        }

        private static string BuildQuery(SortedDictionary<string, string> parameters)
        {
            return string.Join("&", parameters.Select(pair => $"{pair.Key}={Uri.EscapeDataString(pair.Value)}"));
        }

        private string ComputeHash(SortedDictionary<string, string> parameters)
        {
            var rawData = string.Join("&", parameters.Select(pair => $"{pair.Key}={pair.Value}"));
            var secretBytes = Encoding.UTF8.GetBytes(_options.HashSecret);
            using var hmac = new HMACSHA512(secretBytes);
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            return Convert.ToHexString(hashBytes);
        }
    }
}
