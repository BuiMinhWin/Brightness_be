using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using BillingAPI.Application.Interfaces;
using BillingAPI.Application.Requests;
using Microsoft.AspNetCore.Mvc;

namespace BillingAPI.Presentation.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/billing/invoices")]
    public class InvoicesController(IInvoiceService invoiceService) : ControllerBase
    {
        private readonly IInvoiceService _invoiceService = invoiceService;

        [HttpGet]
        public async Task<IActionResult> GetAsync([FromQuery] Guid? userId = null, [FromQuery] string? status = null)
        {
            var invoices = await _invoiceService.GetAllAsync(userId, status);
            return Ok(invoices);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var invoice = await _invoiceService.GetByIdAsync(id);
            return invoice is null ? NotFound() : Ok(invoice);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateInvoiceRequest request)
        {
            var response = await _invoiceService.CreateAsync(request);
            return response.Flag ? Ok(response) : BadRequest(response);
        }

        [HttpPost("{id:guid}/cancel")]
        public async Task<IActionResult> CancelAsync(Guid id, [FromBody] CancelInvoiceRequest request)
        {
            var response = await _invoiceService.CancelAsync(id, request.Reason);
            return response.Flag ? Ok(response) : BadRequest(response);
        }
    }
}
