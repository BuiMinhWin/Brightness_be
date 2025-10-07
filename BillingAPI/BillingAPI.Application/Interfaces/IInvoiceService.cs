using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BillingAPI.Application.DTOs;
using BillingAPI.Application.Requests;
using BrightNess.Library.Responses;

namespace BillingAPI.Application.Interfaces
{
    public interface IInvoiceService
    {
        Task<IEnumerable<InvoiceDTO>> GetAllAsync(Guid? userId = null, string? status = null);
        Task<InvoiceDTO?> GetByIdAsync(Guid id);
        Task<Response> CreateAsync(CreateInvoiceRequest request);
        Task<Response> MarkAsPaidAsync(Guid invoiceId, string? transactionId = null);
        Task<Response> CancelAsync(Guid invoiceId, string? reason = null);
    }
}
