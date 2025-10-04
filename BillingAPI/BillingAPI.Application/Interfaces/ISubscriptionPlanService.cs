using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BillingAPI.Application.DTOs;
using BillingAPI.Application.Requests;
using BrightNess.Library.Responses;

namespace BillingAPI.Application.Interfaces
{
    public interface ISubscriptionPlanService
    {
        Task<IEnumerable<SubscriptionPlanDTO>> GetAllAsync(bool onlyActive = false);
        Task<SubscriptionPlanDTO?> GetByIdAsync(Guid id);
        Task<Response> CreateAsync(CreateSubscriptionPlanRequest request);
        Task<Response> UpdateAsync(Guid id, UpdateSubscriptionPlanRequest request);
        Task<Response> DeleteAsync(Guid id);
    }
}
