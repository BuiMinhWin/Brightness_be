using System;
using System.Threading.Tasks;
using BillingAPI.Application.Requests;
using BillingAPI.Application.Responses;
using BillingAPI.Domain.Enums;
using BrightNess.Library.Responses;

namespace BillingAPI.Application.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentSessionResponse?> InitiateAsync(InitiatePaymentRequest request);
        Task<PaymentStatusResponse?> HandlePayOsWebhookAsync(PayOsWebhookRequest webhookRequest);
        Task<Response> MarkAsFailedAsync(Guid paymentId, string? reason = null);
    }
}
