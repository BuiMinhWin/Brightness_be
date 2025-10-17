using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BillingAPI.Application.DTOs;
using BillingAPI.Application.Interfaces;
using BillingAPI.Application.Requests;
using BillingAPI.Domain.Entities;
using BrightNess.Library.Responses;

namespace BillingAPI.Application.Services
{
    public class InvoiceService(
        IInvoiceRepository invoiceRepository,
        ISubscriptionPlanRepository planRepository,
        IUserSubscriptionService userSubscriptionService,
        IInvoiceEmailService invoiceEmailService)
        : IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepository = invoiceRepository ?? throw new ArgumentNullException(nameof(invoiceRepository));
        private readonly ISubscriptionPlanRepository _planRepository = planRepository ?? throw new ArgumentNullException(nameof(planRepository));
        private readonly IUserSubscriptionService _userSubscriptionService = userSubscriptionService ?? throw new ArgumentNullException(nameof(userSubscriptionService));
        private readonly IInvoiceEmailService _invoiceEmailService = invoiceEmailService ?? throw new ArgumentNullException(nameof(invoiceEmailService));

        public async Task<IEnumerable<InvoiceDTO>> GetAllAsync(Guid? userId = null, string? status = null)
        {
            IEnumerable<Invoice> invoices;
            if (userId.HasValue)
            {
                invoices = await _invoiceRepository.GetByUserAsync(userId.Value);
            }
            else
            {
                invoices = await _invoiceRepository.GetAllAsync();
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                var normalizedStatus = status.Trim().ToLowerInvariant();
                invoices = invoices.Where(invoice => string.Equals(invoice.Status, normalizedStatus, StringComparison.OrdinalIgnoreCase));
            }

            return invoices.Select(MapToDto);
        }

        public async Task<InvoiceDTO?> GetByIdAsync(Guid id)
        {
            var invoice = await _invoiceRepository.FindByIdAsync(id);
            return invoice is null ? null : MapToDto(invoice);
        }

        public async Task<Response> CreateAsync(CreateInvoiceRequest request)
        {
            var plan = await _planRepository.FindByIdAsync(request.SubscriptionPlanId);
            if (plan is null)
            {
                return new Response(false, "Subscription plan not found");
            }

            if (!plan.IsActive)
            {
                return new Response(false, "Subscription plan is inactive");
            }

            var dueDate = request.DueDate ?? DateTime.UtcNow.AddDays(3);
            if (dueDate.Kind == DateTimeKind.Unspecified)
            {
                dueDate = DateTime.SpecifyKind(dueDate, DateTimeKind.Utc);
            }

            var invoice = new Invoice
            {
                Id = Guid.NewGuid(),
                UserExternalId = request.UserExternalId,
                SubscriptionPlanId = plan.Id,
                Amount = plan.Price,
                Currency = plan.Currency,
                Status = "pending",
                InvoiceCode = GenerateInvoiceCode(),
                CreatedAt = DateTime.UtcNow,
                DueDate = dueDate.ToUniversalTime(),
                Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim()
            };

            var createResult = await _invoiceRepository.CreateAsync(invoice);

            // Tr? v? invoice data trong Response.Data d? frontend có th? access invoice.id
            if (createResult.Flag)
            {
                var invoiceDto = MapToDto(invoice);
                return new Response(true, createResult.Message, invoiceDto);
            }

            return createResult;
        }

        public async Task<Response> MarkAsPaidAsync(Guid invoiceId, string? transactionId = null)
        {
            var invoice = await _invoiceRepository.FindByIdAsync(invoiceId);
            if (invoice is null)
            {
                return new Response(false, "Invoice not found");
            }

            if (string.Equals(invoice.Status, "paid", StringComparison.OrdinalIgnoreCase))
            {
                return new Response(true, "Invoice was already paid");
            }

            invoice.Status = "paid";
            invoice.PaidAt = DateTime.UtcNow;
            invoice.UpdatedAt = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(transactionId))
            {
                invoice.Notes = string.IsNullOrWhiteSpace(invoice.Notes)
                    ? $"Paid via transaction {transactionId}"
                    : $"{invoice.Notes}\nPaid via transaction {transactionId}";
            }

            var updateResult = await _invoiceRepository.UpdateAsync(invoice);
            if (!updateResult.Flag)
            {
                return updateResult;
            }

            if (!invoice.UserExternalId.HasValue)
            {
                Console.WriteLine($"?? Cannot update user subscription or send invoice email: UserExternalId is null for invoice {invoiceId}");
                return updateResult;
            }

            var userId = invoice.UserExternalId.Value;

            Console.WriteLine($"?? Calling UserSubscriptionService to update user {userId} with plan {invoice.SubscriptionPlanId}");

            var subscriptionUpdateResult = await _userSubscriptionService.UpdateUserSubscriptionAsync(
                userId,
                invoice.SubscriptionPlanId);

            if (subscriptionUpdateResult)
            {
                Console.WriteLine($"? Successfully updated user subscription for {userId}");
            }
            else
            {
                Console.WriteLine($"?? Failed to update user subscription for {userId}");
            }

            var userProfile = await _userSubscriptionService.GetUserProfileAsync(userId);
            if (userProfile is null)
            {
                Console.WriteLine($"?? Cannot send invoice email: user profile not found for {userId}");
                return updateResult;
            }

            var plan = await _planRepository.FindByIdAsync(invoice.SubscriptionPlanId);
            if (plan is null)
            {
                Console.WriteLine($"?? Cannot send invoice email: subscription plan {invoice.SubscriptionPlanId} not found.");
                return updateResult;
            }

            await _invoiceEmailService.SendInvoicePaidEmailAsync(invoice, plan, userProfile);

            return updateResult;
        }

        public async Task<Response> CancelAsync(Guid invoiceId, string? reason = null)
        {
            var invoice = await _invoiceRepository.FindByIdAsync(invoiceId);
            if (invoice is null)
            {
                return new Response(false, "Invoice not found");
            }

            if (string.Equals(invoice.Status, "paid", StringComparison.OrdinalIgnoreCase))
            {
                return new Response(false, "Cannot cancel a paid invoice");
            }

            invoice.Status = "cancelled";
            invoice.CancelledAt = DateTime.UtcNow;
            invoice.UpdatedAt = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(reason))
            {
                invoice.Notes = string.IsNullOrWhiteSpace(invoice.Notes)
                    ? reason.Trim()
                    : $"{invoice.Notes}\nCancellation reason: {reason.Trim()}";
            }

            return await _invoiceRepository.UpdateAsync(invoice);
        }

        private static InvoiceDTO MapToDto(Invoice invoice) => new(
            invoice.Id,
            invoice.UserExternalId,
            invoice.SubscriptionPlanId,
            invoice.InvoiceCode,
            invoice.Amount,
            invoice.Currency,
            invoice.Status,
            invoice.CreatedAt,
            invoice.DueDate,
            invoice.PaidAt,
            invoice.CancelledAt,
            invoice.UpdatedAt,
            invoice.Notes
        );

        private static string GenerateInvoiceCode()
        {
            var prefix = DateTime.UtcNow.ToString("yyyyMMdd");
            return $"INV-{prefix}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
        }
    }
}

