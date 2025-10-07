using System.ComponentModel.DataAnnotations;

namespace MeasurementAPI.Application.Requests
{
    public class UpdateDeviceRequest
    {
        [MaxLength(128)]
        public string? Manufacturer { get; set; }

        [MaxLength(128)]
        public string? Model { get; set; }

        [MaxLength(128)]
        public string? SerialNumber { get; set; }

        [MaxLength(64)]
        public string? FirmwareVersion { get; set; }

        public bool? IsActive { get; set; }
    }
}
