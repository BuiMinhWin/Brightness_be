using System;
using System.ComponentModel.DataAnnotations;

namespace MeasurementAPI.Application.Requests
{
    public class RecordDeviceSyncRequest
    {
        [Required]
        public Guid DeviceId { get; set; }

        [Required]
        public DateTime SyncStartedAt { get; set; }

        public DateTime? SyncCompletedAt { get; set; }

        public bool Success { get; set; } = true;

        [MaxLength(512)]
        public string? Message { get; set; }

        public string? RawPayload { get; set; }
    }
}
