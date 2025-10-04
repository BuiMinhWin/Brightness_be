using System;
using System.Text.Json.Nodes;

namespace BillingAPI.Application.Requests
{
    public class PayOsWebhookRequest
    {
        public string RawBody { get; set; } = string.Empty;
        public JsonObject? Data { get; set; }
        public string? Signature { get; set; }
    }
}
