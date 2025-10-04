using System;

namespace MeasurementAPI.Application.Requests
{
    public class MeasurementHistoryFilter
    {
        public Guid? DeviceId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public bool IncludeRawData { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
