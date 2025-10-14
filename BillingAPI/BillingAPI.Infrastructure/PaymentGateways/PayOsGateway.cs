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
using BillingAPI.Infrastructure.PaymentGateways.Dtos;
using BillingAPI.Infrastructure.PaymentGateways.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Net.payOS;
using Net.payOS.Types;

namespace BillingAPI.Infrastructure.PaymentGateways
{
    public class PayOsGateway : IPayOsGateway
    {
        private readonly PayOS _payOS;
        private readonly ILogger<PayOsGateway> _logger;
        private readonly PayOsOptions _options;

        public PayOsGateway(IOptions<PayOsOptions> options, ILogger<PayOsGateway> logger)
        {
            var payOsOptions = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = payOsOptions;

            // Log PayOS configuration (mask sensitive data)
            _logger.LogInformation("PayOS Configuration - ClientId: {ClientId}, ApiKey: {ApiKey}, ChecksumKey: {ChecksumKey}, Endpoint: {Endpoint}",
                MaskKey(payOsOptions.ClientId),
                MaskKey(payOsOptions.ApiKey),
                MaskKey(payOsOptions.ChecksumKey),
                payOsOptions.Endpoint);

            // Initialize PayOS SDK with credentials
            _payOS = new PayOS(payOsOptions.ClientId, payOsOptions.ApiKey, payOsOptions.ChecksumKey);
        }
        public async Task<GatewayPaymentResult> CreatePaymentAsync(Invoice invoice, Payment payment, string returnUrl, string? cancelUrl)
        {
            try
            {
                // Use provided URLs or reasonable defaults for testing
                var resolvedReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? "http://localhost:3000/payment-success" : returnUrl;
                var resolvedCancelUrl = string.IsNullOrWhiteSpace(cancelUrl) ? "http://localhost:3000/payment-cancel" : cancelUrl;

                // Generate orderCode using same method as PayOS example
                var orderCode = int.Parse(DateTimeOffset.Now.ToString("ffffff"));

                var amount = (int)Math.Round(payment.Amount, 0, MidpointRounding.AwayFromZero);
                var description = "Thanh toan don hang";

                payment.TransactionId = orderCode.ToString();

                _logger.LogInformation("Creating PayOS payment link: OrderCode={OrderCode}, Amount={Amount}, Description={Description}, CancelUrl={CancelUrl}, ReturnUrl={ReturnUrl}",
                    orderCode, amount, description, resolvedCancelUrl, resolvedReturnUrl);

                // Create payment data using PayOS SDK
                var paymentLinkRequest = new PaymentData(
                    orderCode: orderCode,
                    amount: amount,
                    description: description,
                    items: [new("Subscription", 1, amount)],
                    returnUrl: resolvedReturnUrl,
                    cancelUrl: resolvedCancelUrl
                );

                // Use PayOS SDK to create payment link
                var response = await _payOS.createPaymentLink(paymentLinkRequest);

                _logger.LogInformation("PayOS SDK response: CheckoutUrl={CheckoutUrl}, PaymentLinkId={PaymentLinkId}",
                    response.checkoutUrl, response.paymentLinkId);

                if (string.IsNullOrWhiteSpace(response.checkoutUrl))
                {
                    _logger.LogError("PayOS SDK returned empty checkoutUrl");
                    throw new InvalidOperationException("PayOS SDK returned empty checkout URL");
                }

                // ExpiredAt is not in SDK response, so set a default
                var expiredAt = DateTime.UtcNow.AddMinutes(15);

                return new GatewayPaymentResult(response.checkoutUrl, expiredAt, response.paymentLinkId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating PayOS payment link using SDK");
                throw;
            }
        }

        public Task<bool> ValidateWebhookAsync(string rawBody, string? signature)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(signature))
                {
                    _logger.LogWarning("PayOS webhook missing signature");
                    return Task.FromResult(false);
                }

                var webhookBody = JsonSerializer.Deserialize<WebhookType>(rawBody);
                if (webhookBody is null)
                {
                    _logger.LogWarning("PayOS webhook payload is null or invalid JSON");
                    return Task.FromResult(false);
                }

                // Use PayOS SDK for webhook validation if available
                var result = _payOS.verifyPaymentWebhookData(webhookBody);

                return Task.FromResult(result is not null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating PayOS webhook");
                return Task.FromResult(false);
            }
        }

        public async Task<PaymentStatusResponse?> GetPaymentLinkInfoAsync(string orderCode)
        {
            try
            {
                _logger.LogInformation("Getting payment link info for orderCode: {OrderCode}", orderCode);

                // Use PayOS SDK to get payment link information
                var paymentInfo = await _payOS.getPaymentLinkInformation(long.Parse(orderCode));

                if (paymentInfo is null)
                {
                    _logger.LogWarning("Payment info not found for orderCode: {OrderCode}", orderCode);
                    return null;
                }

                _logger.LogInformation("PayOS payment info retrieved: Status={Status}, Amount={Amount}",
                    paymentInfo.status, paymentInfo.amount);

                // Map PayOS status to our payment status string
                var statusStr = paymentInfo.status?.ToLowerInvariant() switch
                {
                    "paid" => "completed",
                    "pending" => "pending",
                    "cancelled" => "failed",
                    _ => "pending"
                };

                // Determine paidAt
                DateTime? paidAt = statusStr == "completed" ? DateTime.UtcNow : null;

                // Use id field from paymentInfo (SDK uses 'id' instead of 'paymentLinkId')
                var transactionId = paymentInfo.id ?? orderCode;

                // Return response with proper structure (record constructor)
                return new PaymentStatusResponse(
                    PaymentId: Guid.Empty, // Will be filled by caller
                    InvoiceId: Guid.Empty, // Will be filled by caller
                    Gateway: Domain.Enums.PaymentGatewayType.PayOs,
                    PaymentStatus: statusStr,
                    PaidAt: paidAt,
                    TransactionId: transactionId
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment link info for orderCode: {OrderCode}", orderCode);
                return null;
            }
        }

        private string CreateSignature(long orderCode, int amount, string description, string cancelUrl, string returnUrl)
        {
            var data = $"amount={amount}&cancelUrl={cancelUrl}&description={description}&orderCode={orderCode}&returnUrl={returnUrl}";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.ChecksumKey));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }

        private string MaskKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return "[EMPTY]";
            if (key.Length <= 8) return "[MASKED]";
            return $"{key[..4]}****{key[^4..]}";
        }
    }
}
