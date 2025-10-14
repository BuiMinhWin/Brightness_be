using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using BillingAPI.Application.Interfaces;
using BillingAPI.Application.Requests;
using BillingAPI.Application.Responses;
using BillingAPI.Domain.Entities;
using BillingAPI.Domain.Enums;
using BrightNess.Library.Responses;

namespace BillingAPI.Application.Services
{
    public class PaymentService(
        IPaymentRepository paymentRepository,
        IInvoiceRepository invoiceRepository,
        IInvoiceService invoiceService,
        IPayOsGateway payOsGateway) : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
        private readonly IInvoiceRepository _invoiceRepository = invoiceRepository ?? throw new ArgumentNullException(nameof(invoiceRepository));
        private readonly IInvoiceService _invoiceService = invoiceService ?? throw new ArgumentNullException(nameof(invoiceService));
        private readonly IPayOsGateway _payOsGateway = payOsGateway ?? throw new ArgumentNullException(nameof(payOsGateway));

        public async Task<PaymentSessionResponse?> InitiateAsync(InitiatePaymentRequest request)
        {
            var invoice = await _invoiceRepository.FindByIdAsync(request.InvoiceId);
            if (invoice is null || !string.Equals(invoice.Status, "pending", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                InvoiceId = invoice.Id,
                UserExternalId = invoice.UserExternalId,
                Amount = invoice.Amount,
                Currency = invoice.Currency,
                Gateway = request.Gateway,
                PaymentStatus = "pending",
                CreatedAt = DateTime.UtcNow
            };

            // The gateway now modifies the payment object (sets TransactionId), so we pass it and then update it.
            GatewayPaymentResult gatewayResult = request.Gateway switch
            {
                PaymentGatewayType.PayOs => await _payOsGateway.CreatePaymentAsync(invoice, payment, request.ReturnUrl, request.CancelUrl),
                _ => throw new NotSupportedException($"Unsupported payment gateway: {request.Gateway}")
            };

            payment.CheckoutUrl = gatewayResult.PaymentUrl;
            // The actual transactionId from PayOS is different from our internal orderCode
            // We keep our internal orderCode in payment.TransactionId and don't overwrite it here.
            // If needed, a new field could be added for the gateway's specific transaction ID.
            payment.ExpiredAt = gatewayResult.ExpiredAt;

            var createResponse = await _paymentRepository.CreateAsync(payment);
            if (!createResponse.Flag)
            {
                return null;
            }

            return new PaymentSessionResponse(
                payment.Id,
                invoice.Id,
                payment.Gateway,
                payment.CheckoutUrl ?? string.Empty,
                payment.ExpiredAt,
                payment.TransactionId
            );
        }

        public async Task<PaymentStatusResponse?> HandlePayOsWebhookAsync(PayOsWebhookRequest webhookRequest)
        {
            if (webhookRequest.Data is null)
            {
                return null;
            }

            var isValid = await _payOsGateway.ValidateWebhookAsync(webhookRequest.RawBody, webhookRequest.Signature);
            if (!isValid)
            {
                return null;
            }

            var orderCodeStr = webhookRequest.Data["orderCode"]?.ToString();
            if (string.IsNullOrWhiteSpace(orderCodeStr))
            {
                return null;
            }

            // Find the payment by the numeric orderCode stored in TransactionId
            var payment = await _paymentRepository.FindByTransactionIdAsync(orderCodeStr);
            if (payment is null)
            {
                return null;
            }

            var status = webhookRequest.Data["status"]?.ToString()?.ToLowerInvariant();
            // The actual transaction ID from PayOS, which is different from our orderCode
            var gatewayTransactionId = webhookRequest.Data["paymentLinkId"]?.ToString();
            var shouldMarkSuccess = status is "PAID"; // PayOS uses "PAID" for success

            payment.PaymentStatus = shouldMarkSuccess ? "success" : status ?? payment.PaymentStatus;
            // Optionally, store the gateway's transaction ID if you have a field for it.
            // For now, we'll just use our internal one for marking the invoice as paid.
            payment.RawGatewayPayload = webhookRequest.RawBody;
            payment.UpdatedAt = DateTime.UtcNow;

            await _paymentRepository.UpdateAsync(payment);

            if (shouldMarkSuccess)
            {
                await _invoiceService.MarkAsPaidAsync(payment.InvoiceId, gatewayTransactionId ?? payment.TransactionId);
            }

            return new PaymentStatusResponse(
                payment.Id,
                payment.InvoiceId,
                payment.Gateway,
                payment.PaymentStatus,
                shouldMarkSuccess ? DateTime.UtcNow : null,
                gatewayTransactionId ?? payment.TransactionId
            );
        }

        public async Task<Response> MarkAsFailedAsync(Guid paymentId, string? reason = null)
        {
            var payment = await _paymentRepository.FindByIdAsync(paymentId);
            if (payment is null)
            {
                return new Response(false, "Payment not found");
            }

            payment.PaymentStatus = "failed";
            payment.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(reason))
            {
                payment.RawGatewayPayload = string.IsNullOrWhiteSpace(payment.RawGatewayPayload)
                    ? reason
                    : $"{payment.RawGatewayPayload}\n{reason}";
            }

            return await _paymentRepository.UpdateAsync(payment);
        }
    }
}
