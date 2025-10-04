using System;

namespace MeasurementAPI.Application.DTOs
{
    public record MeasurementDeviceDTO
    (
        Guid Id,
        Guid UserId,
        string DeviceType,
        string Manufacturer,
        string Model,
        string? SerialNumber,
        string? FirmwareVersion,
        bool IsActive,
        DateTime RegisteredAt,
        DateTime? LastSyncedAt
    );
}
