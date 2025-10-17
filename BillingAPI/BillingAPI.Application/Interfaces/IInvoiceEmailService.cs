using System.Threading.Tasks;
using BillingAPI.Application.DTOs;
using BillingAPI.Domain.Entities;

namespace BillingAPI.Application.Interfaces
{
    public interface IInvoiceEmailService
    {
        Task SendInvoicePaidEmailAsync(Invoice invoice, SubscriptionPlan plan, UserProfileDTO user);
    }
}
