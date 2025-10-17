using System;

namespace ChatService.Application.DTOs
{
    /// <summary>
    /// DTO containing user subscription information
    /// </summary>
    public record UserSubscriptionDTO
    (
        Guid UserId,
        Guid? SubscriptionPlanId,
        SubscriptionPlanDetailsDTO? PlanDetails
    );
}


