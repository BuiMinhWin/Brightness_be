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
    public class SubscriptionPlanService(ISubscriptionPlanRepository repository) : ISubscriptionPlanService
    {
        private readonly ISubscriptionPlanRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

        public async Task<IEnumerable<SubscriptionPlanDTO>> GetAllAsync(bool onlyActive = false)
        {
            var plans = onlyActive
                ? await _repository.GetActivePlansAsync()
                : await _repository.GetAllAsync();

            return plans.Select(MapToDto);
        }

        public async Task<SubscriptionPlanDTO?> GetByIdAsync(Guid id)
        {
            var plan = await _repository.FindByIdAsync(id);
            return plan is null ? null : MapToDto(plan);
        }

        public async Task<Response> CreateAsync(CreateSubscriptionPlanRequest request)
        {
            var duplicate = await _repository.NameExistsAsync(request.Name);
            if (duplicate)
            {
                return new Response(false, "Subscription plan name already exists");
            }

            var plan = new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim(),
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                Price = request.Price,
                Currency = request.Currency.ToUpperInvariant(),
                DurationDays = request.DurationDays,
                CreditTier = request.CreditTier?.ToLowerInvariant() ?? "free",
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            return await _repository.CreateAsync(plan);
        }

        public async Task<Response> UpdateAsync(Guid id, UpdateSubscriptionPlanRequest request)
        {
            var plan = await _repository.FindByIdAsync(id);
            if (plan is null)
            {
                return new Response(false, "Subscription plan not found");
            }

            if (!string.IsNullOrWhiteSpace(request.Name) && !string.Equals(plan.Name, request.Name, StringComparison.OrdinalIgnoreCase))
            {
                var duplicate = await _repository.NameExistsAsync(request.Name, id);
                if (duplicate)
                {
                    return new Response(false, "Subscription plan name already exists");
                }

                plan.Name = request.Name.Trim();
            }

            if (request.Description is not null)
            {
                plan.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
            }

            if (request.Price.HasValue)
            {
                plan.Price = request.Price.Value;
            }

            if (!string.IsNullOrWhiteSpace(request.Currency))
            {
                plan.Currency = request.Currency.Trim().ToUpperInvariant();
            }

            if (request.DurationDays.HasValue)
            {
                plan.DurationDays = request.DurationDays.Value;
            }

            if (!string.IsNullOrWhiteSpace(request.CreditTier))
            {
                plan.CreditTier = request.CreditTier.Trim().ToLowerInvariant();
            }

            if (request.IsActive.HasValue)
            {
                plan.IsActive = request.IsActive.Value;
            }

            plan.UpdatedAt = DateTime.UtcNow;

            return await _repository.UpdateAsync(plan);
        }

        public async Task<Response> DeleteAsync(Guid id)
        {
            var plan = await _repository.FindByIdAsync(id);
            if (plan is null)
            {
                return new Response(false, "Subscription plan not found");
            }

            return await _repository.DeleteAsync(plan);
        }

        private static SubscriptionPlanDTO MapToDto(SubscriptionPlan plan) => new(
            plan.Id,
            plan.Name,
            plan.Description,
            plan.Price,
            plan.Currency,
            plan.DurationDays,
            plan.CreditTier,
            plan.IsActive,
            plan.CreatedAt,
            plan.UpdatedAt
        );
    }
}
