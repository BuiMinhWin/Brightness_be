using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BillingAPI.Domain.Entities;
using BillingAPI.Domain.Enums;
using BrightNess.Library.Interface;

namespace BillingAPI.Application.Interfaces
{
    public interface IPaymentRepository : IGenericInterface<Payment>
    {
        Task<Payment?> GetLatestForInvoiceAsync(Guid invoiceId, PaymentGatewayType gateway);
        Task<IEnumerable<Payment>> GetPendingPaymentsAsync(DateTime referenceDate);
        Task<Payment?> FindByTransactionIdAsync(string transactionId);
    }
}
