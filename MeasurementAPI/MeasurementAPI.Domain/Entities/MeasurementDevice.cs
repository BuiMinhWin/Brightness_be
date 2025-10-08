using System;
using System.Collections.Generic;

namespace MeasurementAPI.Domain.Entities
{
    public class MeasurementDevice
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string DeviceType { get; set; } = null!;
        public string Manufacturer { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string? SerialNumber { get; set; }
        public string? FirmwareVersion { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime RegisteredAt { get; set; }
        public DateTime? LastSyncedAt { get; set; }
        public DateTime? LastSyncStartedAt { get; set; }
        public DateTime? LastSyncCompletedAt { get; set; }
        public bool? LastSyncSuccess { get; set; }
        public string? LastSyncMessage { get; set; }
        public string? LastSyncPayload { get; set; }

        public ICollection<BodyMeasurement> Measurements { get; set; } = new List<BodyMeasurement>();
    }
}
