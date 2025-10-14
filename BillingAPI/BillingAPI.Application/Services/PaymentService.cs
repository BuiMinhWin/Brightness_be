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

            // Validate webhook signature (skip validation if signature is explicitly "test" for Postman testing)
            var isTestMode = webhookRequest.Signature?.ToLowerInvariant() == "test";
            if (!isTestMode)
            {
                var isValid = await _payOsGateway.ValidateWebhookAsync(webhookRequest.RawBody, webhookRequest.Signature);
                if (!isValid)
                {
                    return null;
                }
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

        public async Task<PaymentStatusResponse?> GetPaymentStatusAsync(Guid paymentId)
        {
            var payment = await _paymentRepository.FindByIdAsync(paymentId);
            if (payment is null)
            {
                return null;
            }

            // Determine paidAt based on payment status
            DateTime? paidAt = payment.PaymentStatus == "completed" || payment.PaymentStatus == "success"
                ? payment.UpdatedAt
                : null;

            return new PaymentStatusResponse(
                payment.Id,
                payment.InvoiceId,
                payment.Gateway,
                payment.PaymentStatus,
                paidAt,
                payment.TransactionId
            );
        }

        public async Task<PaymentStatusResponse?> CheckAndUpdatePaymentStatusAsync(Guid paymentId)
        {
            var payment = await _paymentRepository.FindByIdAsync(paymentId);
            if (payment is null)
            {
                return null;
            }

            // If already completed or failed, return current status
            if (payment.PaymentStatus == "completed" || payment.PaymentStatus == "failed")
            {
                DateTime? paidAt = payment.PaymentStatus == "completed" ? payment.UpdatedAt : null;
                return new PaymentStatusResponse(
                    payment.Id,
                    payment.InvoiceId,
                    payment.Gateway,
                    payment.PaymentStatus,
                    paidAt,
                    payment.TransactionId
                );
            }

            // Check status with PayOS
            if (payment.Gateway == PaymentGatewayType.PayOs && !string.IsNullOrEmpty(payment.TransactionId))
            {
                var payosStatus = await _payOsGateway.GetPaymentLinkInfoAsync(payment.TransactionId);
                if (payosStatus is not null)
                {
                    // Update payment based on PayOS status  
                    payment.PaymentStatus = payosStatus.PaymentStatus;
                    payment.UpdatedAt = DateTime.UtcNow;

                    // Store gateway transaction ID in RawGatewayPayload if completed
                    if (payosStatus.PaymentStatus == "completed")
                    {
                        if (!string.IsNullOrEmpty(payosStatus.TransactionId))
                        {
                            payment.RawGatewayPayload = string.IsNullOrEmpty(payment.RawGatewayPayload)
                                ? $"Gateway TransactionId: {payosStatus.TransactionId}"
                                : $"{payment.RawGatewayPayload}\nGateway TransactionId: {payosStatus.TransactionId}";
                        }
                        await _paymentRepository.UpdateAsync(payment);
                        await _invoiceService.MarkAsPaidAsync(payment.InvoiceId, payosStatus.TransactionId ?? payment.TransactionId);
                    }
                    else
                    {
                        await _paymentRepository.UpdateAsync(payment);
                    }

                    DateTime? paidAt = payosStatus.PaymentStatus == "completed" ? payosStatus.PaidAt : null;
                    return new PaymentStatusResponse(
                        payment.Id,
                        payment.InvoiceId,
                        payment.Gateway,
                        payment.PaymentStatus,
                        paidAt,
                        payment.TransactionId
                    );
                }
            }

            // Return current status if unable to check with gateway
            DateTime? currentPaidAt = payment.PaymentStatus == "completed" ? payment.UpdatedAt : null;
            return new PaymentStatusResponse(
                payment.Id,
                payment.InvoiceId,
                payment.Gateway,
                payment.PaymentStatus,
                currentPaidAt,
                payment.TransactionId
            );
        }
    }
}
