using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BillingAPI.Application.Interfaces;
using BillingAPI.Domain.Entities;
using BillingAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BillingAPI.Infrastructure.Repositories
{
    public class InvoiceRepository : GenericRepository<Invoice>, IInvoiceRepository
    {
        public InvoiceRepository(BillingDbContext context) : base(context)
        {
        }

        public async Task<Invoice?> GetByCodeAsync(string invoiceCode)
        {
            if (string.IsNullOrWhiteSpace(invoiceCode))
            {
                return null;
            }

            return await Context.Invoices
                .Include(invoice => invoice.SubscriptionPlan)
                .Include(invoice => invoice.Payments)
                .AsNoTracking()
                .FirstOrDefaultAsync(invoice => invoice.InvoiceCode == invoiceCode);
        }

        public async Task<IEnumerable<Invoice>> GetByUserAsync(Guid userId)
        {
            return await Context.Invoices
                .Include(invoice => invoice.SubscriptionPlan)
                .Include(invoice => invoice.Payments)
                .AsNoTracking()
                .Where(invoice => invoice.UserExternalId == userId)
                .OrderByDescending(invoice => invoice.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetDueInvoicesAsync(DateTime referenceDate)
        {
            return await Context.Invoices
                .Include(invoice => invoice.SubscriptionPlan)
                .AsNoTracking()
                .Where(invoice => invoice.Status == "pending" && invoice.DueDate <= referenceDate)
                .ToListAsync();
        }

        public override async Task<Invoice?> FindByIdAsync(Guid id)
        {
            return await Context.Invoices
                .Include(invoice => invoice.SubscriptionPlan)
                .Include(invoice => invoice.Payments)
                .FirstOrDefaultAsync(invoice => invoice.Id == id);
        }

        public override async Task<IEnumerable<Invoice>> GetAllAsync()
        {
            return await Context.Invoices
                .Include(invoice => invoice.SubscriptionPlan)
                .Include(invoice => invoice.Payments)
                .AsNoTracking()
                .OrderByDescending(invoice => invoice.CreatedAt)
                .ToListAsync();
        }
    }
}
