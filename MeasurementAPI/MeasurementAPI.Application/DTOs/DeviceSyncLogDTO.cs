using System;

namespace MeasurementAPI.Application.DTOs
{
    public record DeviceSyncLogDTO
    (
        Guid Id,
        Guid DeviceId,
        DateTime SyncStartedAt,
        DateTime? SyncCompletedAt,
        bool Success,
        string? Message
    );
}
