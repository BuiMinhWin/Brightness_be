using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BillingAPI.Application.Interfaces;
using BillingAPI.Application.Responses;
using BillingAPI.Domain.Entities;
using BillingAPI.Infrastructure.PaymentGateways.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BillingAPI.Infrastructure.PaymentGateways
{
    public class PayOsGateway : IPayOsGateway
    {
        private readonly HttpClient _httpClient;
        private readonly PayOsOptions _options;
        private readonly ILogger<PayOsGateway> _logger;

        public PayOsGateway(HttpClient httpClient, IOptions<PayOsOptions> options, ILogger<PayOsGateway> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<GatewayPaymentResult> CreatePaymentAsync(Invoice invoice, Payment payment, string returnUrl, string? cancelUrl)
        {
            var resolvedReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? _options.ReturnUrl : returnUrl;
            var resolvedCancelUrl = string.IsNullOrWhiteSpace(cancelUrl) ? _options.CancelUrl : cancelUrl ?? _options.CancelUrl;

            var payload = new
            {
                orderCode = payment.Id,
                amount = Math.Round(payment.Amount, 0, MidpointRounding.AwayFromZero),
                description = $"Invoice {invoice.InvoiceCode}",
                returnUrl = resolvedReturnUrl,
                cancelUrl = resolvedCancelUrl,
                signature = CreateSignature(payment.Id, payment.Amount, resolvedReturnUrl, resolvedCancelUrl)
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(new Uri(_options.Endpoint), "/payments/create"));
            request.Headers.Add("x-client-id", _options.ClientId);
            request.Headers.Add("x-api-key", _options.ApiKey);
            request.Content = JsonContent.Create(payload);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            using var contentStream = await response.Content.ReadAsStreamAsync();
            using var document = await JsonDocument.ParseAsync(contentStream);
            var root = document.RootElement;
            if (!root.TryGetProperty("data", out var data))
            {
                throw new InvalidOperationException("Unexpected PayOS response format (missing data)");
            }

            var paymentUrl = data.GetProperty("checkoutUrl").GetString() ?? throw new InvalidOperationException("Missing checkoutUrl from PayOS response");
            var expiredAt = data.TryGetProperty("expiredAt", out var expiredAtElement) && expiredAtElement.TryGetDateTime(out var expiry)
                ? expiry
                : (DateTime?)null;

            var transactionId = data.TryGetProperty("transactionId", out var transactionIdElement)
                ? transactionIdElement.GetString()
                : null;

            return new GatewayPaymentResult(paymentUrl, expiredAt, transactionId);
        }

        public Task<bool> ValidateWebhookAsync(string rawBody, string? signature)
        {
            if (string.IsNullOrWhiteSpace(signature))
            {
                _logger.LogWarning("PayOS webhook missing signature");
                return Task.FromResult(false);
            }

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.ChecksumKey));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawBody ?? string.Empty));
            var computedSignature = Convert.ToHexString(hashBytes);
            var isValid = string.Equals(computedSignature, signature, StringComparison.OrdinalIgnoreCase);
            if (!isValid)
            {
                _logger.LogWarning("Invalid PayOS webhook signature");
            }

            return Task.FromResult(isValid);
        }

        private string CreateSignature(Guid orderCode, decimal amount, string returnUrl, string? cancelUrl)
        {
            var rawData = string.Join("|", new[]
            {
                orderCode.ToString(),
                Math.Round(amount, 0, MidpointRounding.AwayFromZero).ToString("F0"),
                returnUrl,
                cancelUrl ?? string.Empty
            });

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.ChecksumKey));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            return Convert.ToHexString(hashBytes);
        }
    }
}
