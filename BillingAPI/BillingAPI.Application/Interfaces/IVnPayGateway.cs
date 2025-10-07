using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BillingAPI.Application.Responses;
using BillingAPI.Domain.Entities;

namespace BillingAPI.Application.Interfaces
{
    public interface IVnPayGateway
    {
        Task<GatewayPaymentResult> CreatePaymentUrlAsync(Invoice invoice, Payment payment, string returnUrl, string? cancelUrl, string? clientIp);
        bool ValidateSignature(Dictionary<string, string> queryParameters);
    }
}
