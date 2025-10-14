using System.Threading.Tasks;
using BillingAPI.Application.Responses;
using BillingAPI.Domain.Entities;

namespace BillingAPI.Application.Interfaces
{
    public interface IPayOsGateway
    {
        Task<GatewayPaymentResult> CreatePaymentAsync(Invoice invoice, Payment payment, string returnUrl, string? cancelUrl);
        Task<bool> ValidateWebhookAsync(string rawBody, string? signature);
        Task<PaymentStatusResponse?> GetPaymentLinkInfoAsync(string orderCode);
    }
}
