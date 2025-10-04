using System;
using System.ComponentModel.DataAnnotations;
using BillingAPI.Domain.Enums;

namespace BillingAPI.Application.Requests
{
    public class InitiatePaymentRequest
    {
        [Required]
        public Guid InvoiceId { get; set; }

        [Required]
        public PaymentGatewayType Gateway { get; set; }

        [Required]
        [Url]
        public string ReturnUrl { get; set; } = null!;

        [Url]
        public string? CancelUrl { get; set; }

        [MaxLength(45)]
        public string? ClientIpAddress { get; set; }
    }
}
