using System;
using System.ComponentModel.DataAnnotations;

namespace MeasurementAPI.Application.Requests
{
    public class RegisterDeviceRequest
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(64)]
        public string DeviceType { get; set; } = null!;

        [MaxLength(128)]
        public string Manufacturer { get; set; } = string.Empty;

        [MaxLength(128)]
        public string Model { get; set; } = string.Empty;

        [MaxLength(128)]
        public string? SerialNumber { get; set; }

        [MaxLength(64)]
        public string? FirmwareVersion { get; set; }
    }
}
