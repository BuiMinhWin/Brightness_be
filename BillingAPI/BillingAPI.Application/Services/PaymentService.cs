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

            GatewayPaymentResult gatewayResult = request.Gateway switch
            {
                PaymentGatewayType.PayOs => await _payOsGateway.CreatePaymentAsync(invoice, payment, request.ReturnUrl, request.CancelUrl),
                _ => throw new NotSupportedException($"Unsupported payment gateway: {request.Gateway}")
            };

            payment.CheckoutUrl = gatewayResult.PaymentUrl;
            payment.TransactionId = gatewayResult.TransactionId;
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

            var orderCode = webhookRequest.Data["orderCode"]?.ToString();
            if (string.IsNullOrWhiteSpace(orderCode) || !Guid.TryParse(orderCode, out var paymentId))
            {
                return null;
            }

            var payment = await _paymentRepository.FindByIdAsync(paymentId);
            if (payment is null)
            {
                return null;
            }

            var status = webhookRequest.Data["status"]?.ToString()?.ToLowerInvariant();
            var transactionId = webhookRequest.Data["transactionId"]?.ToString();
            var shouldMarkSuccess = status is "paid" or "completed";

            payment.PaymentStatus = shouldMarkSuccess ? "success" : status ?? payment.PaymentStatus;
            payment.TransactionId = transactionId ?? payment.TransactionId;
            payment.RawGatewayPayload = webhookRequest.RawBody;
            payment.UpdatedAt = DateTime.UtcNow;

            await _paymentRepository.UpdateAsync(payment);

            if (shouldMarkSuccess)
            {
                await _invoiceService.MarkAsPaidAsync(payment.InvoiceId, payment.TransactionId);
            }

            return new PaymentStatusResponse(
                payment.Id,
                payment.InvoiceId,
                payment.Gateway,
                payment.PaymentStatus,
                shouldMarkSuccess ? DateTime.UtcNow : null,
                payment.TransactionId
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
