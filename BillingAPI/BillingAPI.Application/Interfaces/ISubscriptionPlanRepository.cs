using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BillingAPI.Domain.Entities;
using BrightNess.Library.Interface;

namespace BillingAPI.Application.Interfaces
{
    public interface ISubscriptionPlanRepository : IGenericInterface<SubscriptionPlan>
    {
        Task<bool> NameExistsAsync(string name, Guid? excludeId = null);
        Task<IEnumerable<SubscriptionPlan>> GetActivePlansAsync();
    }
}
