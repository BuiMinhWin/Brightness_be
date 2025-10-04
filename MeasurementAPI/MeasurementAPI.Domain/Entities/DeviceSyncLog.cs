using System;

namespace MeasurementAPI.Domain.Entities
{
    public class DeviceSyncLog
    {
        public Guid Id { get; set; }
        public Guid DeviceId { get; set; }
        public DateTime SyncStartedAt { get; set; }
        public DateTime? SyncCompletedAt { get; set; }
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? RawPayload { get; set; }

        public MeasurementDevice Device { get; set; } = null!;
    }
}
