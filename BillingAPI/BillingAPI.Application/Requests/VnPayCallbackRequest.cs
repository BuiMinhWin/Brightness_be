using System;
using System.Collections.Generic;

namespace BillingAPI.Application.Requests
{
    public class VnPayCallbackRequest
    {
        public VnPayCallbackRequest(Dictionary<string, string> queryParameters, string? clientIp)
        {
            QueryParameters = queryParameters ?? throw new ArgumentNullException(nameof(queryParameters));
            ClientIp = clientIp;
        }

        public Dictionary<string, string> QueryParameters { get; }
        public string? ClientIp { get; }
    }
}
