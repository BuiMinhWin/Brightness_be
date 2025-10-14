using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using BillingAPI.Application.Interfaces;
using BillingAPI.Application.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BillingAPI.Presentation.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/billing/payments")]
    public class PaymentsController(IPaymentService paymentService) : ControllerBase
    {
        private readonly IPaymentService _paymentService = paymentService;

        [HttpPost("initiate")]
        public async Task<IActionResult> InitiateAsync([FromBody] InitiatePaymentRequest request)
        {
            var session = await _paymentService.InitiateAsync(request);
            return session is null ? BadRequest("Unable to initiate payment") : Ok(session);
        }

        [HttpGet("{paymentId:guid}/status")]
        public async Task<IActionResult> GetPaymentStatusAsync(Guid paymentId)
        {
            var status = await _paymentService.GetPaymentStatusAsync(paymentId);
            return status is null ? NotFound("Payment not found") : Ok(status);
        }

        [HttpPost("{paymentId:guid}/check-status")]
        public async Task<IActionResult> CheckAndUpdatePaymentStatusAsync(Guid paymentId)
        {
            var status = await _paymentService.CheckAndUpdatePaymentStatusAsync(paymentId);
            return status is null ? NotFound("Payment not found") : Ok(status);
        }

        [AllowAnonymous]
        [HttpPost("payos/webhook")]
        public async Task<IActionResult> PayOsWebhookAsync()
        {
            Request.EnableBuffering();
            using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
            var rawBody = await reader.ReadToEndAsync();
            Request.Body.Position = 0;

            var jsonNode = string.IsNullOrWhiteSpace(rawBody) ? null : JsonNode.Parse(rawBody) as JsonObject;
            var signature = Request.Headers["x-payos-signature"].ToString();

            var webhookRequest = new PayOsWebhookRequest
            {
                RawBody = rawBody,
                Data = jsonNode,
                Signature = string.IsNullOrWhiteSpace(signature) ? null : signature
            };

            var result = await _paymentService.HandlePayOsWebhookAsync(webhookRequest);
            return result is null ? BadRequest("Invalid PayOS webhook") : Ok(result);
        }
    }
}
