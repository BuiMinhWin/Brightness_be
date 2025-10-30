using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BillingAPI.Application.DTOs;
using BillingAPI.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BillingAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AICreditsController : ControllerBase
    {
        private readonly ICreditManagementService _creditManagementService;
        private readonly ILogger<AICreditsController> _logger;

        public AICreditsController(
            ICreditManagementService creditManagementService,
            ILogger<AICreditsController> logger)
        {
            _creditManagementService = creditManagementService;
            _logger = logger;
        }

        /// <summary>
        /// Get AI credit balance for a user and service
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="serviceName">Service name (chat or schedule)</param>
        [HttpGet("{userId}/balance/{serviceName}")]
        public async Task<ActionResult<AICreditBalanceDTO>> GetBalance(Guid userId, string serviceName)
        {
            try
            {
                var balance = await _creditManagementService.GetCreditBalanceAsync(userId, serviceName);
                return Ok(new { success = true, data = balance });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting credit balance for user {UserId}, service {ServiceName}", 
                    userId, serviceName);
                return StatusCode(500, new { success = false, message = "Failed to get credit balance", error = ex.Message });
            }
        }

        /// <summary>
        /// Deduct credits for an AI operation
        /// </summary>
        [HttpPost("deduct")]
        public async Task<ActionResult<CreditOperationResult>> DeductCredits([FromBody] DeductCreditsRequest request)
        {
            try
            {
                var result = await _creditManagementService.DeductCreditsAsync(request);
                
                if (!result.Success)
                {
                    return StatusCode(429, new { 
                        success = false, 
                        message = result.Message,
                        remainingTokens = result.RemainingTokens 
                    });
                }

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deducting credits for user {UserId}, service {ServiceName}", 
                    request.UserId, request.ServiceName);
                return StatusCode(500, new { success = false, message = "Failed to deduct credits", error = ex.Message });
            }
        }

        /// <summary>
        /// Check if user can afford an operation
        /// </summary>
        [HttpGet("{userId}/can-afford")]
        public async Task<ActionResult<bool>> CanAfford(Guid userId, [FromQuery] string serviceName, [FromQuery] int estimatedTokens)
        {
            try
            {
                var canAfford = await _creditManagementService.CanAffordOperationAsync(userId, serviceName, estimatedTokens);
                return Ok(new { success = true, canAfford });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking affordability for user {UserId}", userId);
                return StatusCode(500, new { success = false, message = "Failed to check affordability", error = ex.Message });
            }
        }

        /// <summary>
        /// Get usage history for a user
        /// </summary>
        [HttpGet("{userId}/history")]
        public async Task<ActionResult<List<AIOperationLogDTO>>> GetHistory(
            Guid userId, 
            [FromQuery] string? serviceName = null, 
            [FromQuery] int limit = 50)
        {
            try
            {
                var history = await _creditManagementService.GetUsageHistoryAsync(userId, serviceName, limit);
                return Ok(new { success = true, data = history, count = history.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting usage history for user {UserId}", userId);
                return StatusCode(500, new { success = false, message = "Failed to get usage history", error = ex.Message });
            }
        }

        /// <summary>
        /// Initialize credit pools for a user
        /// </summary>
        [HttpPost("{userId}/initialize")]
        public async Task<IActionResult> InitializePools(Guid userId, [FromQuery] string subscriptionTier = "free")
        {
            try
            {
                await _creditManagementService.InitializeCreditPoolsForUserAsync(userId, subscriptionTier);
                return Ok(new { success = true, message = "Credit pools initialized successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing credit pools for user {UserId}", userId);
                return StatusCode(500, new { success = false, message = "Failed to initialize credit pools", error = ex.Message });
            }
        }
    }
}

