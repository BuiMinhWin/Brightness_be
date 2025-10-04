using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BillingAPI.Domain.Entities;
using BrightNess.Library.Interface;

namespace BillingAPI.Application.Interfaces
{
    public interface IInvoiceRepository : IGenericInterface<Invoice>
    {
        Task<Invoice?> GetByCodeAsync(string invoiceCode);
        Task<IEnumerable<Invoice>> GetByUserAsync(Guid userId);
        Task<IEnumerable<Invoice>> GetDueInvoicesAsync(DateTime referenceDate);
    }
}
