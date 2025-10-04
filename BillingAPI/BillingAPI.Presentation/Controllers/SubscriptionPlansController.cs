using System;
using System.Threading.Tasks;
using BillingAPI.Application.Interfaces;
using BillingAPI.Application.Requests;
using Microsoft.AspNetCore.Mvc;

namespace BillingAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/billing/plans")]
    public class SubscriptionPlansController(ISubscriptionPlanService subscriptionPlanService) : ControllerBase
    {
        private readonly ISubscriptionPlanService _subscriptionPlanService = subscriptionPlanService;

        [HttpGet]
        public async Task<IActionResult> GetAsync([FromQuery] bool onlyActive = false)
        {
            var plans = await _subscriptionPlanService.GetAllAsync(onlyActive);
            return Ok(plans);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var plan = await _subscriptionPlanService.GetByIdAsync(id);
            return plan is null ? NotFound() : Ok(plan);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateSubscriptionPlanRequest request)
        {
            var response = await _subscriptionPlanService.CreateAsync(request);
            return response.Flag ? Ok(response) : BadRequest(response);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateSubscriptionPlanRequest request)
        {
            var response = await _subscriptionPlanService.UpdateAsync(id, request);
            return response.Flag ? Ok(response) : BadRequest(response);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            var response = await _subscriptionPlanService.DeleteAsync(id);
            return response.Flag ? Ok(response) : BadRequest(response);
        }
    }
}
