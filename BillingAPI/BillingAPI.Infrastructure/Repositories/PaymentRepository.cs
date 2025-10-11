using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BillingAPI.Application.Interfaces;
using BillingAPI.Domain.Entities;
using BillingAPI.Domain.Enums;
using BillingAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BillingAPI.Infrastructure.Repositories
{
    public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
    {
        public PaymentRepository(BillingDbContext context) : base(context)
        {
        }

        public async Task<Payment?> GetLatestForInvoiceAsync(Guid invoiceId, PaymentGatewayType gateway)
        {
            return await Context.Payments
                .AsNoTracking()
                .Where(payment => payment.InvoiceId == invoiceId && payment.Gateway == gateway)
                .OrderByDescending(payment => payment.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Payment>> GetPendingPaymentsAsync(DateTime referenceDate)
        {
            return await Context.Payments
                .AsNoTracking()
                .Where(payment => payment.PaymentStatus == "pending" && (payment.ExpiredAt == null || payment.ExpiredAt >= referenceDate))
                .ToListAsync();
        }

        public async Task<Payment?> FindByTransactionIdAsync(string transactionId)
        {
            return await Context.Payments
                .FirstOrDefaultAsync(p => p.TransactionId == transactionId);
        }

        public override async Task<Payment?> FindByIdAsync(Guid id)
        {
            return await Context.Payments
                .Include(payment => payment.Invoice)
                .ThenInclude(invoice => invoice.SubscriptionPlan)
                .FirstOrDefaultAsync(payment => payment.Id == id);
        }
    }
}
